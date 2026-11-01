"""Durable LangGraph checkpoints through the .NET EF Core boundary, not a second ORM."""
import base64
import json
from threading import Condition
from langgraph.checkpoint.base import BaseCheckpointSaver, CheckpointTuple, WRITES_IDX_MAP
from ..errors import PipelineError


class EfCheckpointSaver(BaseCheckpointSaver):
    def __init__(self, callback, thread_id):
        super().__init__()
        self.callback = callback
        self.thread_id = thread_id
        self.persisted = set()
        self.ready = Condition()

    def config(self, checkpoint_id):
        return {"configurable": {"thread_id": self.thread_id, "checkpoint_ns": "", "checkpoint_id": checkpoint_id}}

    def pack(self, value):
        kind, data = self.serde.dumps_typed(value)
        return {"type": kind, "data": base64.b64encode(data).decode("ascii")}

    def unpack(self, value):
        # Only the pinned serializer's JSON/msgpack forms; never deserialize pickle.
        if value["type"] not in ("json", "msgpack", "bytes", "null"):
            raise PipelineError("Kiểu checkpoint không được hỗ trợ.")
        return self.serde.loads_typed((value["type"], base64.b64decode(value["data"], validate=True)))

    def get_tuple(self, config):
        checkpoint_id = config["configurable"].get("checkpoint_id")
        response = self.callback("GET", "/checkpoint", params={"checkpointId": checkpoint_id} if checkpoint_id else {})
        if not response:
            return None
        row = response["checkpoint"]; payload = json.loads(row["payload"])
        with self.ready:
            self.persisted.add(row["checkpointId"])
            self.ready.notify_all()
        writes = []
        for write in response["writes"]:
            entry = json.loads(write["payload"])
            writes.append((write["taskId"], entry["channel"], self.unpack(entry["value"])))
        return CheckpointTuple(config=self.config(row["checkpointId"]), checkpoint=self.unpack(payload["checkpoint"]),
            metadata=self.unpack(payload["metadata"]), parent_config=self.config(row["parentId"]) if row["parentId"] else None,
            pending_writes=writes)

    def put(self, config, checkpoint, metadata, new_versions):
        if config["configurable"].get("checkpoint_ns", ""):
            raise PipelineError("Graph hiện tại không dùng subgraph namespaces.")
        self.callback("POST", "/checkpoint", json={"checkpointId": checkpoint["id"],
            "parentId": config["configurable"].get("checkpoint_id"),
            "payload": json.dumps({"checkpoint": self.pack(checkpoint), "metadata": self.pack(metadata)})})
        with self.ready:
            self.persisted.add(checkpoint["id"])
            self.ready.notify_all()
        return self.config(checkpoint["id"])

    def put_writes(self, config, writes, task_id, task_path=""):
        # LangGraph can schedule writes concurrently with put; EF enforces the checkpoint FK.
        with self.ready:
            if not self.ready.wait_for(lambda: config["configurable"]["checkpoint_id"] in self.persisted, timeout=60):
                raise PipelineError("Chưa lưu được checkpoint trước khi lưu kết quả bước xử lý. Bạn thử lại nhé.")
        self.callback("POST", "/writes", json={"checkpointId": config["configurable"]["checkpoint_id"],
            "writes": [{"taskId": task_id, "index": WRITES_IDX_MAP.get(channel, index),
                "payload": json.dumps({"channel": channel, "value": self.pack(value), "taskPath": task_path})}
                for index, (channel, value) in enumerate(writes)]})
