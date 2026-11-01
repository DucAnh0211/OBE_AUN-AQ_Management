def search_documents(callback, plan):
    return callback("POST", "/retrieve", json={"question": plan["question"], "queries": plan["queries"]})
