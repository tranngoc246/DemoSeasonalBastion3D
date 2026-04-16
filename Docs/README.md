# DemoSeasonalBastion3D Docs

Đây là bộ docs làm việc chính cho **project 3D đang được tiếp tục chỉnh sửa**: `DemoSeasonalBastion3D`.

## Scope của thư mục này

Mục tiêu của docs ở đây là:
- giữ định hướng migration từ bản 2D sang bản 3D
- theo dõi tiến độ implementation của runtime 3D
- khóa các ràng buộc kiến trúc để 3D không trở thành gameplay authority mới
- giúp tiếp tục công việc nhất quán dù đang mở project trên 2 máy khác nhau

## Path convention

Hiện có 2 máy đang dùng song song, nên docs có thể nhắc đến **cả 2 path**:
- `C:\UnityProjects\SeasonalBastionV2`
- `E:\Projects\SeasonalBastionV2`
- `C:\UnityProjects\DemoSeasonalBastion3D`
- `E:\Projects\DemoSeasonalBastion3D`

Coi chúng là **hai bản path tương ứng của cùng project** trên hai máy khác nhau, không phải hai project khác nhau.

## Source of truth

### `V3D_Migration_Checklist.md`
Đây là **working doc chính**.
Dùng file này làm nguồn truth cho:
- planning hiện tại
- progress tracking
- implementation priorities
- trạng thái các phase

### `V3D_Migration_Baseline.md`
Đây là **baseline reference**.
Dùng file này để giữ chắc:
- assumption về project 2D gốc
- protected gameplay modules
- scene/layout assumptions
- migration constraints

Nếu có cảm giác checklist và baseline lệch nhau, ưu tiên xử lý như sau:
1. dùng **Baseline** để bảo vệ assumption kiến trúc và gameplay authority
2. dùng **Checklist** để quyết định việc đang làm tiếp theo
3. nếu phát hiện lệch thật, cập nhật checklist cho khớp baseline hoặc cập nhật cả hai có chủ đích

## Archived files

Archived vì đã mang tính lịch sử hoặc đã được gộp vào checklist chính:
- `Archive/V3D_Implementation_Priority.md`
- `Archive/T02_Spatial_Audit.md`

## Recommended working set

Khi quay lại project sau một thời gian, thường chỉ cần đọc lại:
- `V3D_Migration_Checklist.md`
- `V3D_Migration_Baseline.md`

## Tóm tắt ngắn

Nếu chỉ nhớ một điều:

**Docs trong thư mục này phục vụ project 3D `DemoSeasonalBastion3D`, còn `SeasonalBastionV2` chỉ là gameplay reference. Mọi chỉnh sửa thực tế nên tiếp tục ở project 3D.**
