# ERD: học vụ (schema `curriculum`)

Phạm vi: tổ chức, CTĐT, học phần, lớp học phần, sinh viên, giảng viên và điểm. Bảng CLO/PLO đầy đủ xem các migration `InitialCurriculum`. Mô tả nghiệp vụ: [SHARED_ACADEMIC_GRADEBOOK.md](../SHARED_ACADEMIC_GRADEBOOK.md).

```mermaid
erDiagram
    universities ||--o{ org_units : "khoa, bộ môn"
    org_units ||--o{ org_units : "khoa → bộ môn"
    universities ||--|| grading_policies : "quy chế"
    universities ||--o{ grade_scales : "thang điểm chữ/hệ 4"
    universities ||--o{ programs : ""
    org_units |o--o{ programs : "khoa quản lý ngành"
    universities ||--o{ courses : "danh mục học phần"
    org_units |o--o{ courses : "bộ môn quản lý"
    org_units |o--o{ lecturer_profiles : "bộ môn của GV"
    programs ||--o{ program_versions : ""
    program_versions ||--o{ program_courses : "học phần trong CTĐT"
    courses ||--o{ program_courses : ""
    program_courses ||--o{ clos : ""
    program_courses ||--o{ assessment_components : "đề cương đánh giá"
    assessment_components ||--o{ assessment_component_clos : ""
    clos ||--o{ assessment_component_clos : "đầu điểm đo CLO"
    program_versions ||--o{ home_classes : "lớp sinh hoạt theo khóa"
    home_classes |o--o{ academic_students : ""
    universities ||--o{ academic_students : "MSSV duy nhất/trường"
    program_courses ||--o{ course_offerings : "lớp học phần / học kỳ"
    home_classes |o--o{ course_offerings : "mở riêng cho lớp"
    course_offerings ||--o{ offering_lecturers : "GV chính, trợ giảng, TH"
    course_offerings ||--o{ course_enrollments : "sổ điểm"
    academic_students ||--o{ course_enrollments : "lần học"
    course_enrollments ||--o{ student_grades : ""
    assessment_components ||--o{ student_grades : ""
    course_enrollments ||--o{ grade_changes : "lịch sử, phúc khảo"

    org_units { bigint Id PK  bigint UniversityId  bigint ParentId  string Kind  string Code  string Name }
    grading_policies { bigint UniversityId PK  int CourseScoreDecimals  int MinComponents  decimal MinFinalWeight  string RetakePolicy  int GpaDecimals }
    grade_scales { bigint Id PK  bigint UniversityId  string Letter  decimal MinScore  decimal Point4  bool IsPassing }
    lecturer_profiles { bigint UserId PK  string LecturerCode UK  bigint DepartmentId  string AcademicTitle  string Degree }
    program_courses { bigint id PK  int credits  bool counts_toward_gpa  int grade_scheme_revision }
    assessment_components { bigint Id PK  bigint ProgramCourseId  string Code  string Kind  decimal Weight  decimal MaxScore }
    assessment_component_clos { bigint ComponentId PK  bigint CloId PK  decimal Weight }
    home_classes { bigint Id PK  bigint ProgramVersionId  string CohortCode  string Code  bigint AdvisorUserId }
    academic_students { bigint Id PK  string StudentCode  string FullName  string CohortCode  string Status  bigint HomeClassId  bigint UserId UK }
    course_offerings { bigint Id PK  string Code UK  string AcademicPeriod  string GroupName  int Capacity  string GradeStatus  datetime PublishedAt }
    offering_lecturers { bigint OfferingId PK  bigint UserId PK  string Role }
    course_enrollments { bigint Id PK  string AttemptKind  string SpecialMark  decimal FinalScore  string LetterGrade  decimal GradePoint  bool Passed  datetime PublishedAt }
    student_grades { bigint EnrollmentId PK  bigint ComponentId PK  decimal Score }
    grade_changes { bigint Id PK  decimal PreviousScore  decimal NewScore  bigint ActorId  string Source  string Reason }
```

Ràng buộc chính:

- `course_offerings`: duy nhất `(ProgramCourseId, AcademicPeriod, GroupName)`; mã 6 số từ sequence `course_offering_codes`; `GradeStatus ∈ {draft, submitted, published}`.
- `offering_lecturers`: tối đa một `Role = 'main'` mỗi lớp học phần (unique index có điều kiện).
- `course_enrollments`: duy nhất `(OfferingId, StudentId)`; FK ghép `(OfferingId, ProgramCourseId, AcademicPeriod)` tới lớp học phần.
- `assessment_components`: duy nhất `(ProgramCourseId, Code)`; `assessment_component_clos` xóa theo khi xóa đầu điểm hoặc CLO.
- `UserId`, `AdvisorUserId`, `ActorId` là ID tài khoản ở schema `identity` (khác DbContext nên không có FK); phân quyền đọc `offering_lecturers` qua read model của identity.
