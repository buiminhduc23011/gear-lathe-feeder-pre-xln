using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Alarms;

namespace Desktop.App.Configuration.Alarms;

public static class AlarmDefinitionCatalog
{
    public const string DefaultDescription = "Chưa cập nhật mô tả";
    public const string DefaultRemedy = "Chưa cập nhật hướng dẫn khắc phục";
    public const string PlcDisconnectedKey = "system.plc_disconnected";

    public static IReadOnlyList<AlarmDefinition> All { get; } = BuildDefinitions();

    public static void Validate()
    {
        var uniqueKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in All)
        {
            if (!uniqueKeys.Add(definition.Key))
            {
                throw new InvalidOperationException($"Alarm definition key '{definition.Key}' is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(definition.Title))
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' is missing Title.");
            }

            if (definition.SourceType != AlarmSourceType.System && string.IsNullOrWhiteSpace(definition.TagName))
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' is missing TagName.");
            }

            if (definition.SourceType != AlarmSourceType.System && string.IsNullOrWhiteSpace(definition.Address))
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' is missing Address.");
            }

            if (definition.SourceType == AlarmSourceType.CodeWord && !definition.MatchAnyPositiveCode && definition.CodeValue is null)
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' must declare CodeValue.");
            }
        }
    }

    public static string NormalizeDescription(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? DefaultDescription : value.Trim();
    }

    public static string NormalizeRemedy(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? DefaultRemedy : value.Trim();
    }

    private static IReadOnlyList<AlarmDefinition> BuildDefinitions()
    {
        var definitions = new List<AlarmDefinition>
        {
            CreateSystem(
                PlcDisconnectedKey,
                "Mất kết nối PLC",
                "PLC_DISCONNECTED",
                "Không nhận được dữ liệu runtime từ PLC.",
                "Kiểm tra nguồn PLC, cáp mạng, IP/Port, sau đó kết nối lại."),

            CreateCodeSlot(
                "alarm.slot_1",
                PlcTagCatalog.Alarms.AlarmCode1,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cảnh báo từ PLC - Slot 1",
                "PLC đang phát mã cảnh báo trong thanh ghi D5140.",
                "Đối chiếu mã cảnh báo trên HMI/chương trình PLC và xử lý theo tài liệu máy.",
                suppress: true),

            CreateCodeSlot(
                "alarm.slot_2",
                PlcTagCatalog.Alarms.AlarmCode2,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cảnh báo từ PLC - Slot 2",
                "PLC đang phát mã cảnh báo trong thanh ghi D5141.",
                "Đối chiếu mã cảnh báo trên HMI/chương trình PLC và xử lý theo tài liệu máy.",
                suppress: true),

            CreateCodeSlot(
                "error.slot_1",
                PlcTagCatalog.Alarms.ErrorCode1,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 1",
                "PLC đang phát mã lỗi trong thanh ghi D5142.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_2",
                PlcTagCatalog.Alarms.ErrorCode2,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 2",
                "PLC đang phát mã lỗi trong thanh ghi D5143.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_3",
                PlcTagCatalog.Alarms.ErrorCode3,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 3",
                "PLC đang phát mã lỗi trong thanh ghi D5144.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_4",
                PlcTagCatalog.Alarms.ErrorCode4,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 4",
                "PLC đang phát mã lỗi trong thanh ghi D5145.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_5",
                PlcTagCatalog.Alarms.ErrorCode5,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 5",
                "PLC đang phát mã lỗi trong thanh ghi D5146.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_6",
                PlcTagCatalog.Alarms.ErrorCode6,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 6",
                "PLC đang phát mã lỗi trong thanh ghi D5147.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            CreateCodeSlot(
                "error.slot_7",
                PlcTagCatalog.Alarms.ErrorCode7,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi từ PLC - Slot 7",
                "PLC đang phát mã lỗi trong thanh ghi D5148.",
                "Xác định mã lỗi, đưa máy về trạng thái an toàn, xử lý rồi reset lỗi.",
                stopMachine: true,
                suppress: true),

            // D5138 (Thêm mới 10/09/2026)
            CreateBit(
                "alarm.lathe_op1_interlock_door",
                PlcTagCatalog.Alarms.LatheOp1InterlockDoor,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Máy Tiện OP1 đang chế độ Interlock cửa",
                "Máy tiện OP1 đang ở chế độ Interlock cửa.",
                "Kiểm tra khóa Interlock máy Op1"),
            CreateBit(
                "alarm.lathe_op2_interlock_door",
                PlcTagCatalog.Alarms.LatheOp2InterlockDoor,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Máy Tiện OP2 đang chế độ Interlock cửa",
                "Máy tiện OP2 đang ở chế độ Interlock cửa.",
                "Kiểm tra khóa Interlock máy Op2"),
            CreateBit(
                "alarm.input_group_manual_mode",
                PlcTagCatalog.Alarms.InputGroupManualMode,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm Input đang chế độ Bằng Tay",
                "Cụm Input đang ở chế độ Bằng Tay.",
                "Kiểm tra và chuyển cụm Input về chế độ Tự Động."),
            CreateBit(
                "alarm.input_group_paused",
                PlcTagCatalog.Alarms.InputGroupPaused,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm Input đang tạm dừng",
                "Cụm Input đang trong trạng thái tạm dừng.",
                "Kiểm tra trạng thái cụm Input và tiếp tục chu trình."),
            CreateBit(
                "alarm.output_group_1_manual_mode",
                PlcTagCatalog.Alarms.OutputGroup1ManualMode,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm Output 1 đang chế độ Bằng Tay",
                "Cụm Output 1 đang ở chế độ Bằng Tay.",
                "Kiểm tra và chuyển cụm Output 1 về chế độ Tự Động."),
            CreateBit(
                "alarm.output_group_1_paused",
                PlcTagCatalog.Alarms.OutputGroup1Paused,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm Output 1 đang tạm dừng",
                "Cụm Output 1 đang trong trạng thái tạm dừng.",
                "Kiểm tra trạng thái cụm Output 1 và tiếp tục chu trình."),
            CreateBit(
                "alarm.output_group_2_manual_mode",
                PlcTagCatalog.Alarms.OutputGroup2ManualMode,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm Output 2 đang chế độ Bằng Tay",
                "Cụm Output 2 đang ở chế độ Bằng Tay.",
                "Kiểm tra và chuyển cụm Output 2 về chế độ Tự Động."),
            CreateBit(
                "alarm.output_group_2_paused",
                PlcTagCatalog.Alarms.OutputGroup2Paused,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm Output 2 đang tạm dừng",
                "Cụm Output 2 đang trong trạng thái tạm dừng.",
                "Kiểm tra trạng thái cụm Output 2 và tiếp tục chu trình."),
            CreateBit(
                "alarm.conveyor_1_manual_mode",
                PlcTagCatalog.Alarms.Conveyor1ManualMode,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm băng tải 1 đang chế độ Bằng Tay",
                "Cụm băng tải 1 đang ở chế độ Bằng Tay.",
                "Kiểm tra và chuyển cụm băng tải 1 về chế độ Tự Động."),
            CreateBit(
                "alarm.conveyor_1_input_paused",
                PlcTagCatalog.Alarms.Conveyor1InputPaused,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm băng tải 1 Input đang tạm dừng",
                "Cụm băng tải 1 Input đang trong trạng thái tạm dừng.",
                "Kiểm tra trạng thái cụm băng tải 1 Input và tiếp tục chu trình."),
            CreateBit(
                "alarm.conveyor_2_manual_mode",
                PlcTagCatalog.Alarms.Conveyor2ManualMode,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm băng tải 2 đang chế độ Bằng Tay",
                "Cụm băng tải 2 đang ở chế độ Bằng Tay.",
                "Kiểm tra và chuyển cụm băng tải 2 về chế độ Tự Động."),
            CreateBit(
                "alarm.conveyor_2_input_paused",
                PlcTagCatalog.Alarms.Conveyor2InputPaused,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Cụm băng tải 2 Input đang tạm dừng",
                "Cụm băng tải 2 Input đang trong trạng thái tạm dừng.",
                "Kiểm tra trạng thái cụm băng tải 2 Input và tiếp tục chu trình."),
            CreateBit(
                "alarm.estop",
                PlcTagCatalog.Alarms.EStop,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi dừng khẩn cấp",
                "Nút dừng khẩn cấp đang kích hoạt.",
                "Reset nút EMG và kiểm tra chuỗi an toàn.", stopMachine: true),
            CreateBit(
                "alarm.x_soft_limit_outside",
                PlcTagCatalog.Alarms.XSoftLimitOutside,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn mềm ngoài trục X",
                "Trục X vượt quá giới hạn mềm phía ngoài.",
                "Di chuyển trục X vào vùng an toàn.", stopMachine: true),
            CreateBit(
                "alarm.x_soft_limit_inside",
                PlcTagCatalog.Alarms.XSoftLimitInside,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn mềm trong trục X",
                "Trục X vượt quá giới hạn mềm phía trong.",
                "Di chuyển trục X vào vùng an toàn.", stopMachine: true),
            CreateBit(
                "alarm.z_soft_limit_bottom",
                PlcTagCatalog.Alarms.ZSoftLimitBottom,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn mềm dưới trục Z",
                "Trục Z vượt quá giới hạn mềm phía dưới.",
                "Di chuyển trục Z lên vùng an toàn.", stopMachine: true),
            CreateBit(
                "alarm.z_soft_limit_top",
                PlcTagCatalog.Alarms.ZSoftLimitTop,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn mềm trên trục Z",
                "Trục Z vượt quá giới hạn mềm phía trên.",
                "Di chuyển trục Z xuống vùng an toàn.", stopMachine: true),
            CreateBit(
                "alarm.not_homed_rodal",
                PlcTagCatalog.Alarms.NotHomedRodal,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Lỗi chưa về gốc Rodal",
                "Tay Rodal chưa thực hiện về gốc.",
                "Thực hiện về gốc cho tay Rodal."),
            CreateBit(
                "alarm.alarm_driver_x",
                PlcTagCatalog.Alarms.AlarmDriverX,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi Driver trục X",
                "Driver trục X báo lỗi.",
                "Kiểm tra mã lỗi trên Driver trục X.", stopMachine: true),
            CreateBit(
                "alarm.alarm_driver_z",
                PlcTagCatalog.Alarms.AlarmDriverZ,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi Driver trục Z",
                "Driver trục Z báo lỗi.",
                "Kiểm tra mã lỗi trên Driver trục Z.", stopMachine: true),
            CreateBit(
                "alarm.canopen_disconnect",
                PlcTagCatalog.Alarms.CanOpenDisconnect,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Mất kết nối CANopen",
                "Mạng CANopen không còn giao tiếp với driver trục X hoặc trục Z.",
                "Kiểm tra nguồn driver, dây truyền thông, đầu nối và khởi động lại mạng CANopen; sau đó về gốc trục X, Z.", stopMachine: true),
            CreateBit(
                "alarm.x_limit_negative",
                PlcTagCatalog.Alarms.XLimitNegative,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn cứng ngoài trục X",
                "Trục X chạm giới hạn cứng phía ngoài.",
                "Di chuyển trục X thủ công ra khỏi công tắc giới hạn.", stopMachine: true),
            CreateBit(
                "alarm.x_limit_positive",
                PlcTagCatalog.Alarms.XLimitPositive,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn cứng trong trục X",
                "Trục X chạm giới hạn cứng phía trong.",
                "Di chuyển trục X thủ công ra khỏi công tắc giới hạn.", stopMachine: true),
            CreateBit(
                "alarm.z_limit_negative",
                PlcTagCatalog.Alarms.ZLimitNegative,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn cứng dưới trục Z",
                "Trục Z chạm giới hạn cứng phía dưới.",
                "Di chuyển trục Z thủ công ra khỏi công tắc giới hạn.", stopMachine: true),
            CreateBit(
                "alarm.z_limit_positive",
                PlcTagCatalog.Alarms.ZLimitPositive,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi Giới hạn cứng trên trục Z",
                "Trục Z chạm giới hạn cứng phía trên.",
                "Di chuyển trục Z thủ công ra khỏi công tắc giới hạn.", stopMachine: true),
            CreateBit(
                "alarm.x_over_moment",
                PlcTagCatalog.Alarms.XOverMoment,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi quá Momen trục X",
                "Momen trục X vượt ngưỡng cho phép.",
                "Kiểm tra Kẹt cơ khí trục X.", stopMachine: true),
            CreateBit(
                "alarm.z_over_moment",
                PlcTagCatalog.Alarms.ZOverMoment,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi quá Momen trục Z",
                "Momen trục Z vượt ngưỡng cho phép.",
                "Kiểm tra Kẹt cơ khí trục Z.", stopMachine: true),
            CreateBit(
                "alarm.air_pressure_lost",
                PlcTagCatalog.Alarms.AirPressureLost,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi áp suất khí nén mức thấp",
                "Áp suất khí nén cấp cho hệ thống bị sụt giảm.",
                "Kiểm tra nguồn khí cấp.", stopMachine: true),
            CreateBit(
                "alarm.rotate_cylinder_timeout_0",
                PlcTagCatalog.Alarms.RotateCylinderTimeout0,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi TimeOut Xilanh xoay 0 cụm Tool",
                "Xilanh xoay 0 cụm Tool không đạt vị trí đúng thời gian.",
                "Kiểm tra cảm biến và áp suất khí xilanh xoay Tool."),
            CreateBit(
                "alarm.rotate_cylinder_timeout_180",
                PlcTagCatalog.Alarms.RotateCylinderTimeout180,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi TimeOut Xilanh xoay 180 cụm Tool",
                "Xilanh xoay 180 cụm Tool không đạt vị trí đúng thời gian.",
                "Kiểm tra cảm biến và áp suất khí xilanh xoay Tool."),
            CreateBit(
                "alarm.tool_check_sensor_1",
                PlcTagCatalog.Alarms.ToolCheckSensor1,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi cảm biến check hàng tool 1",
                "Cảm biến phôi tool 1 báo lỗi hoặc không có hàng.",
                "Kiểm tra cảm biến phôi tool 1."),
            CreateBit(
                "alarm.tool_check_sensor_2",
                PlcTagCatalog.Alarms.ToolCheckSensor2,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi cảm biến check hàng tool 2",
                "Cảm biến phôi tool 2 báo lỗi hoặc không có hàng.",
                "Kiểm tra cảm biến phôi tool 2."),
            CreateBit(
                "alarm.model_collision_at_flip_position",
                PlcTagCatalog.Alarms.ModelCollisionAtFlipPosition,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi thông số Model va chạm vị trí đảo hàng",
                "Thông số Model làm vị trí gắp/thả có nguy cơ va chạm khối V tại vị trí đảo chiều phôi.",
                "Kiểm tra cài đặt Model và chỉnh lại offset tọa độ Z gắp/thả để vị trí dưới nam châm không va chạm khối V.", stopMachine: true),

            // D5142.12 - D5142.15 (Thêm mới 10/09/2026)
            CreateBit(
                "alarm.x_safe_zone_over_limit",
                PlcTagCatalog.Alarms.XSafeZoneOverLimit,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi quá giới hạn vùng an toàn trục X",
                "Trục X vượt quá giới hạn vùng an toàn.",
                "Kiểm tra lại vị trí Trục X, Tọa độ hiện tại, kiểm tra vị trí an toàn và đưa trục X về vùng an toàn",
                stopMachine: true),
            CreateBit(
                "alarm.z_safe_zone_over_limit",
                PlcTagCatalog.Alarms.ZSafeZoneOverLimit,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi quá giới hạn vùng an toàn trục Z",
                "Trục Z vượt quá giới hạn vùng an toàn.",
                "Kiểm tra lại vị trí Trục Z, Tọa độ hiện tại, kiểm tra vị trí an toàn và đưa trục Z về vùng an toàn",
                stopMachine: true),
            CreateBit(
                "alarm.flip_position_clamp_timeout",
                PlcTagCatalog.Alarms.FlipPositionClampTimeout,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi Timeout Xilanh kẹp vị trí đảo hàng",
                "Xilanh kẹp tại vị trí đảo hàng không đạt vị trí đúng thời gian.",
                "Kiểm tra xilanh kẹp vị trí đảo hàng, van khí, áp suất khí và cảm biến."),
            CreateBit(
                "alarm.flip_position_unclamp_timeout",
                PlcTagCatalog.Alarms.FlipPositionUnclampTimeout,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi Timeout Xilanh mở kẹp vị trí đảo hàng",
                "Xilanh mở kẹp tại vị trí đảo hàng không đạt vị trí đúng thời gian.",
                "Kiểm tra xilanh mở kẹp vị trí đảo hàng, van khí, áp suất khí và cảm biến."),
            CreateBit(
                "alarm.lifter_soft_limit_bottom",
                PlcTagCatalog.Alarms.LifterSoftLimitBottom,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn mềm dưới cụm nâng",
                "Cụm nâng chạm giới hạn mềm phía dưới.",
                "Di chuyển cụm nâng lên vị trí an toàn.", stopMachine: true),
            CreateBit(
                "alarm.lifter_soft_limit_top",
                PlcTagCatalog.Alarms.LifterSoftLimitTop,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn mềm trên cụm nâng",
                "Cụm nâng chạm giới hạn mềm phía trên.",
                "Di chuyển cụm nâng xuống vị trí an toàn.", stopMachine: true),
            CreateBit(
                "alarm.lifter_inverter_fault",
                PlcTagCatalog.Alarms.LifterInverterFault,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi biến tần cụm nâng",
                "Biến tần cụm nâng báo lỗi.",
                "Kiểm tra mã lỗi trên biến tần cụm nâng.", stopMachine: true),
            CreateBit(
                "alarm.rotary_inverter_fault",
                PlcTagCatalog.Alarms.RotaryInverterFault,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi biến tần bàn xoay",
                "Biến tần bàn xoay báo lỗi.",
                "Kiểm tra mã lỗi trên biến tần bàn xoay.", stopMachine: true),
            CreateBit(
                "alarm.input_group_not_homed",
                PlcTagCatalog.Alarms.InputGroupNotHomed,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Lỗi chưa về gốc Cụm Input",
                "Cụm Input chưa được thực hiện về gốc.",
                "Thực hiện về gốc Cụm Input."),
            CreateBit(
                "alarm.lifter_pulse_slip",
                PlcTagCatalog.Alarms.LifterPulseSlip,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi trượt xung cụm nâng",
                "Phát hiện sai lệch xung encoder cụm nâng.",
                "Kiểm tra encoder và cơ cấu truyền động cụm nâng."),
            CreateBit(
                "alarm.rotary_pulse_slip",
                PlcTagCatalog.Alarms.RotaryPulseSlip,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi trượt xung bàn xoay",
                "Phát hiện sai lệch xung encoder bàn xoay.",
                "Kiểm tra encoder và cơ cấu truyền động bàn xoay."),
            CreateBit(
                "alarm.lifter_hard_limit_bottom",
                PlcTagCatalog.Alarms.LifterHardLimitBottom,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn cứng dưới cụm nâng",
                "Cụm nâng chạm công tắc hành trình dưới.",
                "Di chuyển cụm nâng thủ công lên trên.", stopMachine: true),
            CreateBit(
                "alarm.lifter_hard_limit_top",
                PlcTagCatalog.Alarms.LifterHardLimitTop,
                AlarmType.Error,
                AlarmSeverity.Critical,
                "Lỗi giới hạn cứng trên cụm nâng",
                "Cụm nâng chạm công tắc hành trình trên.",
                "Di chuyển cụm nâng thủ công xuống dưới.", stopMachine: true),
            CreateBit(
                "alarm.cart_clamp_timeout",
                PlcTagCatalog.Alarms.CartClampTimeout,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi Timeout Xilanh kẹp xe",
                "Xilanh kẹp xe không phản hồi tín hiệu kẹp đúng thời gian.",
                "Kiểm tra xilanh kẹp xe và cảm biến."),
            CreateBit(
                "alarm.cart_unclamp_timeout",
                PlcTagCatalog.Alarms.CartUnclampTimeout,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi Timeout Xilanh mở kẹp xe",
                "Xilanh mở kẹp xe không phản hồi tín hiệu đúng thời gian.",
                "Kiểm tra xilanh mở kẹp xe và cảm biến."),
            CreateBit(
                "alarm.cart_position_invalid",
                PlcTagCatalog.Alarms.CartPositionInvalid,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lôi sai vị trí xe",
                "Tín hiệu kiểm tra vị trí xe không chính xác.",
                "Kiểm tra lại vị trí xe đòn nâng."),
            CreateBit(
                "alarm.rotary_not_at_home",
                PlcTagCatalog.Alarms.RotaryNotAtHome,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Lỗi Bàn xoay không ở gốc",
                "Tín hiệu sensor gốc bàn xoay không kích hoạt.",
                "Thực hiện về gốc bàn xoay."),
            CreateBit(
                "alarm.motor_not_at_home",
                PlcTagCatalog.Alarms.MotorNotAtHome,
                AlarmType.Alarm,
                AlarmSeverity.Warning,
                "Lỗi Motor không ở gốc",
                "Tín hiệu sensor gốc Motor không kích hoạt.",
                "Thực hiện về gốc Motor."),
            CreateBit(
                "alarm.input_rotate_timeout_0",
                PlcTagCatalog.Alarms.InputRotateTimeout0,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi TimeOut XL xoay 0 cụm input",
                "Xilanh xoay 0 cụm input không đạt vị trí.",
                "Kiểm tra xilanh xoay 0 cụm input."),
            CreateBit(
                "alarm.input_rotate_timeout_90",
                PlcTagCatalog.Alarms.InputRotateTimeout90,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi TimeOut XL xoay 90 cụm input",
                "Xilanh xoay 90 cụm input không đạt vị trí.",
                "Kiểm tra xilanh xoay 90 cụm input."),
            CreateBit(
                "alarm.input_clamp_timeout",
                PlcTagCatalog.Alarms.InputClampTimeout,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi Timout kẹp phôi cụm Input",
                "Xilanh kẹp phôi cụm input không đạt vị trí.",
                "Kiểm tra xilanh kẹp phôi cụm input."),
            CreateBit(
                "alarm.input_unclamp_timeout",
                PlcTagCatalog.Alarms.InputUnclampTimeout,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi Timout mở kẹp phôi cụm Input",
                "Xilanh mở kẹp phôi cụm input không đạt vị trí.",
                "Kiểm tra xilanh mở kẹp phôi cụm input."),
            CreateBit(
                "alarm.input_check_sensor_fault",
                PlcTagCatalog.Alarms.InputCheckSensorFault,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi cảm biến check hàng đầu vào",
                "Cảm biến check hàng đầu vào không nhận diện đúng.",
                "Kiểm tra cảm biến check hàng đầu vào."),

            // D5144.12 - D5144.14 (Thêm mới 10/09/2026)
            CreateBit(
                "alarm.motor_lift_timeout",
                PlcTagCatalog.Alarms.MotorLiftTimeout,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi TimeOut Nâng động cơ",
                "Cơ cấu nâng động cơ không đạt vị trí đúng thời gian.",
                "Kiểm tra xilanh nâng động cơ, van khí, áp suất khí và cảm biến."),
            CreateBit(
                "alarm.motor_lower_timeout",
                PlcTagCatalog.Alarms.MotorLowerTimeout,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi Timeout Hạ động cơ",
                "Cơ cấu hạ động cơ không đạt vị trí đúng thời gian.",
                "Kiểm tra xilanh hạ động cơ, van khí, áp suất khí và cảm biến."),
            CreateBit(
                "alarm.rotary_not_at_position",
                PlcTagCatalog.Alarms.RotaryNotAtPosition,
                AlarmType.Error,
                AlarmSeverity.Warning,
                "Lỗi bàn xoay chưa quay đúng vị trí",
                "Bàn xoay chưa quay đến đúng vị trí yêu cầu.",
                "Kiểm tra vị trí bàn xoay, cảm biến và động cơ bàn xoay."),
        };

        ValidateDefinitions(definitions);
        return definitions;
    }

    private static void ValidateDefinitions(IEnumerable<AlarmDefinition> definitions)
    {
        var uniqueKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            if (!uniqueKeys.Add(definition.Key))
            {
                throw new InvalidOperationException($"Alarm definition key '{definition.Key}' is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(definition.Title))
            {
                throw new InvalidOperationException($"Alarm definition '{definition.Key}' is missing Title.");
            }
        }
    }

    private static AlarmDefinition CreateBit(
        string key,
        PlcTagDefinition tag,
        AlarmType alarmType,
        AlarmSeverity severity,
        string title,
        string? description,
        string? remedy,
        bool stopMachine = false)
    {
        return new AlarmDefinition
        {
            Key = key,
            TagName = tag.Name,
            Address = tag.Address,
            SourceType = AlarmSourceType.Bit,
            AlarmType = alarmType,
            Severity = severity,
            Title = title,
            Description = description,
            Remedy = remedy,
            StopMachine = stopMachine,
            ActiveWhenTrue = true,
        };
    }

    private static AlarmDefinition CreateCodeSlot(
        string key,
        PlcTagDefinition tag,
        AlarmType alarmType,
        AlarmSeverity severity,
        string title,
        string? description,
        string? remedy,
        bool stopMachine = false,
        bool suppress = false)
    {
        return new AlarmDefinition
        {
            Key = key,
            TagName = tag.Name,
            Address = tag.Address,
            SourceType = AlarmSourceType.CodeWord,
            AlarmType = alarmType,
            Severity = severity,
            Title = title,
            Description = description,
            Remedy = remedy,
            StopMachine = stopMachine,
            MatchAnyPositiveCode = true,
            Suppress = suppress,
        };
    }

    private static AlarmDefinition CreateSystem(
        string key,
        string title,
        string address,
        string? description,
        string? remedy)
    {
        return new AlarmDefinition
        {
            Key = key,
            TagName = key,
            Address = address,
            SourceType = AlarmSourceType.System,
            AlarmType = AlarmType.System,
            Severity = AlarmSeverity.Critical,
            Title = title,
            Description = description,
            Remedy = remedy,
            StopMachine = true,
        };
    }
}
