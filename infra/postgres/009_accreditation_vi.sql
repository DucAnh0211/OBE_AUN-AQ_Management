BEGIN;

-- Vietnamese reference wording supplied in the Computer Science SAR. This is
-- intentionally kept as draft because AUN has not published it as an official
-- Vietnamese translation of the Programme Assessment Guide v4.0.
WITH vi_statements(code, statement) AS (
    VALUES
      ('1.1','Kết quả học tập mong đợi của CTĐT được xây dựng theo thang đo tư duy được lựa chọn, tương thích với tầm nhìn và sứ mạng của CSGD, được phổ biến đến các bên liên quan.'),
      ('1.2','Kết quả học tập mong đợi của tất cả các học phần được xây dựng phù hợp và tương thích với kết quả học tập mong đợi của CTĐT.'),
      ('1.3','Kết quả học tập mong đợi của CTĐT bao gồm các chuẩn tổng quát (liên quan đến kỹ năng giao tiếp trực tiếp hay qua văn bản, giải quyết vấn đề, sử dụng công nghệ thông tin, làm việc nhóm…) và chuẩn chuyên ngành (liên quan đến kiến thức và kỹ năng của ngành đào tạo).'),
      ('1.4','Yêu cầu của các bên liên quan, đặc biệt là đối tượng bên ngoài được thu thập và chuyển tải vào kết quả học tập mong đợi.'),
      ('1.5','CTĐT thể hiện người học đạt được kết quả học tập mong đợi tại thời điểm tốt nghiệp.'),
      ('2.1','Bản mô tả CTĐT và đề cương tất cả các học phần đầy đủ thông tin, cập nhật, được công bố công khai và các bên liên quan dễ dàng tiếp cận.'),
      ('2.2','Chương trình dạy học được thiết kế tương thích với kết quả học tập mong đợi.'),
      ('2.3','Thông tin phản hồi của các bên liên quan, đặc biệt là các đối tượng bên ngoài, được sử dụng làm căn cứ để thiết kế và phát triển CTDH.'),
      ('2.4','Mức độ đóng góp của mỗi học phần trong việc đạt được kết quả học tập mong đợi được xác định rõ ràng.'),
      ('2.5','CTDH có cấu trúc logic, trình tự hợp lý (các học phần được sắp xếp từ kiến thức cơ bản đến cơ sở và chuyên ngành) và có tính tích hợp.'),
      ('2.6','CTDH cho phép người học lựa chọn chuyên ngành chính và/hoặc các chuyên ngành phụ.'),
      ('2.7','CTDH được rà soát định kỳ theo quy trình để đảm bảo tính cập nhật và đáp ứng yêu cầu của thị trường lao động.'),
      ('3.1','Triết lý giáo dục được tuyên bố rõ ràng, được phổ biến đến tất cả các bên liên quan và được chuyển tải vào các hoạt động dạy và học.'),
      ('3.2','Các hoạt động dạy và học tạo điều kiện cho người học tham gia quá trình học một cách có trách nhiệm.'),
      ('3.3','Các hoạt động dạy và học được triển khai theo hướng tạo điều kiện cho người học học tập chủ động.'),
      ('3.4','Các hoạt động dạy và học khuyến khích người học học tập, học phương pháp học và thấm nhuần yêu cầu học tập suốt đời (VD: tư duy phản biện, kỹ năng xử lý thông tin và sẵn lòng thử nghiệm các ý tưởng và cách làm mới).'),
      ('3.5','Các hoạt động dạy và học giúp người học thấm nhuần tầm quan trọng của việc đưa ra các sáng kiến, tư duy sáng tạo, đổi mới và tinh thần khởi nghiệp.'),
      ('3.6','Quá trình dạy và học được cải tiến liên tục để đảm bảo đáp ứng nhu cầu của thị trường lao động và tương thích với kết quả học tập mong đợi.'),
      ('4.1','Các phương pháp đánh giá kết quả học tập của người học được sử dụng đa dạng; được thiết kế phù hợp với kết quả học tập mong đợi và các mục tiêu đào tạo.'),
      ('4.2','Các chính sách về đánh giá kết quả học tập, phúc khảo được phát biểu rõ ràng, phổ biến đến người học và được triển khai nhất quán.'),
      ('4.3','Các tiêu chuẩn và quy trình liên quan đến đánh giá kết quả học tập của người học trong suốt quá trình theo học và khi hoàn thành chương trình được phát biểu rõ ràng, phổ biến đến người học và được triển khai nhất quán.'),
      ('4.4','Các phương pháp đánh giá kết quả học tập của người học bao gồm bảng tiêu chí đánh giá, thang điểm, các mốc thời gian và các quy định được sử dụng để đảm bảo độ giá trị, độ tin cậy và sự công bằng của hoạt động kiểm tra đánh giá.'),
      ('4.5','Các phương pháp đánh giá giúp đo lường mức độ đạt được kết quả học tập của CTĐT và mỗi học phần.'),
      ('4.6','Thông tin phản hồi kết quả đánh giá kết quả học tập được gửi kịp thời.'),
      ('4.7','Hoạt động đánh giá kết quả học tập của người học và các quy trình có liên quan được rà soát và cải tiến liên tục để đảm bảo sự phù hợp với nhu cầu của thị trường lao động và tương thích với kết quả học tập mong đợi.'),
      ('5.1','Việc quy hoạch đội ngũ GV (bao gồm các kế hoạch kế nhiệm, nâng bậc/thăng chức, bố trí lại, chấm dứt hợp đồng và cho nghỉ hưu) được thực hiện nhằm đảm bảo đội ngũ GV đáp ứng nhu cầu các hoạt động đào tạo, nghiên cứu khoa học, phục vụ cộng đồng về cả số lượng và chất lượng.'),
      ('5.2','Tải trọng công việc của GV được đo lường và giám sát để cải tiến chất lượng các hoạt động đào tạo, nghiên cứu khoa học, phục vụ cộng đồng.'),
      ('5.3','Năng lực của GV được xác định, được đánh giá và được phổ biến thông tin.'),
      ('5.4','GV được phân công nhiệm vụ phù hợp với trình độ, kinh nghiệm và khả năng.'),
      ('5.5','Có hệ thống đánh giá để khen thưởng GV, trong đó có xem xét hoạt động giảng dạy, nghiên cứu và phục vụ cộng đồng.'),
      ('5.6','Các quyền, đặc quyền, quyền lợi, vai trò, mối quan hệ và trách nhiệm giải trình của GV được xác định và hiểu rõ, trong đó có xem xét đến quyền tự do học thuật và đạo đức nghề nghiệp.'),
      ('5.7','Có hệ thống xác định nhu cầu về đào tạo, phát triển chuyên môn của GV và các hoạt động đào tạo, tập huấn phù hợp được triển khai để đáp ứng những nhu cầu này.'),
      ('5.8','Công tác quản lý để đánh giá chất lượng giảng dạy, nghiên cứu khoa học và phục vụ cộng đồng của giảng viên, nghiên cứu viên bao gồm cả việc khen thưởng và công nhận được triển khai.'),
      ('6.1','Chính sách tuyển sinh, tiêu chí tuyển chọn và quy trình tiếp nhận người học vào chương trình được xác định rõ ràng, được ban hành, phổ biến rộng rãi và cập nhật.'),
      ('6.2','Công tác quy hoạch ngắn hạn và dài hạn đối với đội ngũ cán bộ hỗ trợ (học thuật, phi học thuật) được triển khai nhằm đảm bảo đáp ứng nhu cầu của hoạt động đào tạo, nghiên cứu và phục vụ cộng đồng cả về chất lượng và số lượng.'),
      ('6.3','Có hệ thống phù hợp để giám sát tiến độ học tập, kết quả học và tải trọng học tập của người học. Tiến độ học tập, kết quả học tập và tải trọng học tập của người học được ghi nhận và giám sát một cách có hệ thống; có phản hồi tới người học và có các hoạt động khắc phục được triển khai khi cần.'),
      ('6.4','Các hoạt động tư vấn học tập, các hoạt động ngoại khóa, thi đua và những dịch vụ hỗ trợ khác được triển khai nhằm giúp nâng cao chất lượng học tập và khả năng tìm được việc làm cho người học.'),
      ('6.5','Năng lực của cán bộ hỗ trợ triển khai các dịch vụ dành cho người học được xác định rõ trong tiêu chí tuyển dụng và phân công nhiệm vụ. Các năng lực này được đánh giá để đảm bảo phù hợp với nhu cầu của các bên liên quan. Vai trò và mối liên hệ được xác định rõ để đảm bảo các dịch vụ được triển khai nhịp nhàng.'),
      ('6.6','Các dịch vụ hỗ trợ người học được đánh giá, đối sánh và cải tiến chất lượng.'),
      ('7.1','Có đủ các nguồn lực cơ sở vật chất bao gồm các trang thiết bị, tài nguyên học tập và hệ thống công nghệ thông tin để vận hành chương trình.'),
      ('7.2','Các phòng thí nghiệm và trang thiết bị được cập nhật, sẵn có và được sử dụng hiệu quả.'),
      ('7.3','Có thư viện điện tử được cập nhật thường xuyên bắt kịp với những tiến bộ về công nghệ thông tin-truyền thông.'),
      ('7.4','Có hệ thống công nghệ thông tin đáp ứng nhu cầu của cán bộ GV và người học.'),
      ('7.5','Cán bộ, GV và người học dễ dàng tiếp cận với hệ thống mạng và máy tính trong khuôn viên của trường để có thể khai thác tối đa công nghệ thông tin phục vụ các hoạt động giảng dạy, nghiên cứu, phục vụ cộng đồng và quản lý hành chính.'),
      ('7.6','Các tiêu chuẩn về môi trường, sức khỏe và an toàn được xác định và thực hiện; có lưu ý đến các nhu cầu đặc thù của người khuyết tật.'),
      ('7.7','CSGD cung cấp môi trường tâm lý, xã hội, cảnh quan thuận lợi cho hoạt động đào tạo, nghiên cứu và tạo sự thoải mái cho người học.'),
      ('7.8','Năng lực của cán bộ hỗ trợ tham gia các dịch vụ liên quan đến cơ sở vật chất và trang thiết bị được xác định rõ và đánh giá nhằm đảm bảo những kỹ năng đáp ứng nhu cầu các bên liên quan.'),
      ('7.9','Chất lượng các cơ sở vật chất (như thư viện, phòng thực hành, thí nghiệm, CNTT và các dịch vụ dành cho người học) được đánh giá và cải tiến.'),
      ('8.1','Tỷ lệ đậu, tỷ lệ thôi học và thời gian tốt nghiệp trung bình được thiết lập, theo dõi, đối sánh để cải tiến chất lượng.'),
      ('8.2','Tình hình việc làm cũng như số liệu về tự kinh doanh, khởi nghiệp và tiếp tục học cao hơn của người học được thiết lập, theo dõi, đối sánh để cải tiến chất lượng.'),
      ('8.3','Dữ liệu về hoạt động nghiên cứu khoa học và các sản phẩm, hoạt động sáng chế do GV và người học thực hiện được thiết lập, theo dõi, đối sánh để cải tiến chất lượng.'),
      ('8.4','Có dữ liệu về mức độ đạt được kết quả học tập mong đợi CTĐT của người học. Dữ liệu này được thiết lập và theo dõi.'),
      ('8.5','Mức độ hài lòng của các bên liên quan cần được thiết lập, theo dõi, đối sánh để cải tiến chất lượng.')
)
INSERT INTO accreditation.aun_requirement_translations
    (requirement_id, language_code, statement, translation_status, translation_source)
SELECT r.id, 'vi', vi_statements.statement, 'draft',
       'SAR Khoa học máy tính do đơn vị cung cấp; bản tiếng Việt tham khảo, không phải bản dịch chính thức của AUN'
FROM accreditation.aun_requirements r
JOIN accreditation.aun_criteria c ON c.id = r.criterion_id
JOIN accreditation.aun_frameworks f ON f.id = c.framework_id
JOIN vi_statements ON vi_statements.code = r.code
WHERE f.code = 'AUN-QA-PROGRAMME' AND f.version = '4.0'
ON CONFLICT (requirement_id, language_code) DO UPDATE
SET statement = EXCLUDED.statement,
    translation_status = EXCLUDED.translation_status,
    translation_source = EXCLUDED.translation_source,
    updated_at = now()
WHERE aun_requirement_translations.translation_status IN ('draft', 'machine_translated')
  AND aun_requirement_translations.translation_source =
      'SAR Khoa học máy tính do đơn vị cung cấp; bản tiếng Việt tham khảo, không phải bản dịch chính thức của AUN';

COMMIT;
