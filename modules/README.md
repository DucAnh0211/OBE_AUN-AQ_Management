# Modules

Các module nghiệp vụ được chia theo phạm vi chịu trách nhiệm:

```text
modules/
├── common/
│   └── curriculum/       # SV4 và SV5 cùng phát triển
├── sv4/
│   └── accreditation/    # Phần riêng của SV4
└── sv5/
    └── reporting/        # Phần riêng của SV5
```

`common` chứa nghiệp vụ dùng chung của sản phẩm. Thư mục `shared` ở cấp gốc chỉ
chứa hạ tầng kỹ thuật, contract và tiện ích tái sử dụng; không chứa business
rule của các module.

Module dùng chung phải được cả SV4 và SV5 review khi thay đổi public contract.
Module riêng chỉ được truy cập module khác qua public entry point hoặc contract.
