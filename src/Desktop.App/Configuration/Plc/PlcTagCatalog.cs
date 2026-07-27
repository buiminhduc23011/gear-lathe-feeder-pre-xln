using System.Reflection;

namespace Desktop.App.Configuration.Plc;

public static class PlcTagCatalog
{
    public static class Words
    {
        public static readonly PlcTagDefinition D5120 = new()
        {
            Name = "word.d5120",
            Address = "D5120",
            DataType = PlcTagDataType.Int16,
            Description = "Word input group for X0.00 - X0.15",
        };

        public static readonly PlcTagDefinition D5121 = new()
        {
            Name = "word.d5121",
            Address = "D5121",
            DataType = PlcTagDataType.Int16,
            Description = "Word input group for X1.00 - X1.15",
        };

        public static readonly PlcTagDefinition D5122 = new()
        {
            Name = "word.d5122",
            Address = "D5122",
            DataType = PlcTagDataType.Int16,
            Description = "Word input group for X2.00 - X2.15",
        };

        public static readonly PlcTagDefinition D5123 = new()
        {
            Name = "word.d5123",
            Address = "D5123",
            DataType = PlcTagDataType.Int16,
            Description = "Word input group for X3.00 - X3.15",
        };

        public static readonly PlcTagDefinition D5124 = new()
        {
            Name = "word.d5124",
            Address = "D5124",
            DataType = PlcTagDataType.Int16,
            Description = "Word input group for X4.00 - X4.15",
        };

        public static readonly PlcTagDefinition D5130 = new()
        {
            Name = "word.d5130",
            Address = "D5130",
            DataType = PlcTagDataType.Int16,
            Description = "Word output group for Y0.00 - Y0.15",
        };

        public static readonly PlcTagDefinition D5131 = new()
        {
            Name = "word.d5131",
            Address = "D5131",
            DataType = PlcTagDataType.Int16,
            Description = "Word output group for Y1.00 - Y1.15",
        };
    }

    public static class Inputs
    {
        // --- Word 0: X0.00 - X0.15 ---
        public static readonly PlcTagDefinition X0_00 = CreateBit("input.x0_00", "X0.00", "Encoder Bàn Xoay Pha A");
        public static readonly PlcTagDefinition X0_01 = CreateBit("input.x0_01", "X0.01", "Encoder Bàn Xoay Pha B");
        public static readonly PlcTagDefinition X0_02 = CreateBit("input.x0_02", "X0.02", "Encoder Cụm Nâng Pha A");
        public static readonly PlcTagDefinition X0_03 = CreateBit("input.x0_03", "X0.03", "Encoder Cụm Nâng Pha B");
        public static readonly PlcTagDefinition X0_04 = CreateBit("input.x0_04", "X0.04", "X0.4");
        public static readonly PlcTagDefinition X0_05 = CreateBit("input.x0_05", "X0.05", "X0.5");
        public static readonly PlcTagDefinition X0_06 = CreateBit("input.x0_06", "X0.06", "X0.6");
        public static readonly PlcTagDefinition X0_07 = CreateBit("input.x0_07", "X0.07", "X0.7");
        public static readonly PlcTagDefinition X0_08 = CreateBit("input.x0_08", "X0.08", "X0.8");
        public static readonly PlcTagDefinition X0_09 = CreateBit("input.x0_09", "X0.09", "X0.9");
        public static readonly PlcTagDefinition X0_10 = CreateBit("input.x0_10", "X0.10", "X0.10");
        public static readonly PlcTagDefinition X0_11 = CreateBit("input.x0_11", "X0.11", "X0.11");
        public static readonly PlcTagDefinition X0_12 = CreateBit("input.x0_12", "X0.12", "CB XL Trước Nam Châm Cụm out ở trong");
        public static readonly PlcTagDefinition X0_13 = CreateBit("input.x0_13", "X0.13", "CB XL Trước Nam Châm Cụm out ở ngoài");
        public static readonly PlcTagDefinition X0_14 = CreateBit("input.x0_14", "X0.14", "CB XL Nâng Motor");
        public static readonly PlcTagDefinition X0_15 = CreateBit("input.x0_15", "X0.15", "CB XL Hạ Motor");

        // --- Word 1: X1.00 - X1.15 ---
        public static readonly PlcTagDefinition X1_00 = CreateBit("input.x1_00", "X1.00", "Đồng hồ áp khí");
        public static readonly PlcTagDefinition X1_01 = CreateBit("input.x1_01", "X1.01", "Dừng khẩn cấp");
        public static readonly PlcTagDefinition X1_02 = CreateBit("input.x1_02", "X1.02", "Switch Auto/man");
        public static readonly PlcTagDefinition X1_03 = CreateBit("input.x1_03", "X1.03", "Nút bắt đầu");
        public static readonly PlcTagDefinition X1_04 = CreateBit("input.x1_04", "X1.04", "Nút tạm dừng");
        public static readonly PlcTagDefinition X1_05 = CreateBit("input.x1_05", "X1.05", "Nút xóa lỗi");
        public static readonly PlcTagDefinition X1_06 = CreateBit("input.x1_06", "X1.06", "Nút về gốc");
        public static readonly PlcTagDefinition X1_07 = CreateBit("input.x1_07", "X1.07", "X1.7");
        public static readonly PlcTagDefinition X1_08 = CreateBit("input.x1_08", "X1.08", "CB XL kẹp xe trái");
        public static readonly PlcTagDefinition X1_09 = CreateBit("input.x1_09", "X1.09", "CB XL Mở xe trái");
        public static readonly PlcTagDefinition X1_10 = CreateBit("input.x1_10", "X1.10", "CB XL kẹp xe phải");
        public static readonly PlcTagDefinition X1_11 = CreateBit("input.x1_11", "X1.11", "CB XL mở xe phải");
        public static readonly PlcTagDefinition X1_12 = CreateBit("input.x1_12", "X1.12", "CB check xe trái");
        public static readonly PlcTagDefinition X1_13 = CreateBit("input.x1_13", "X1.13", "CB check xe phải");
        public static readonly PlcTagDefinition X1_14 = CreateBit("input.x1_14", "X1.14", "CB Gốc Motor bàn xoay");
        public static readonly PlcTagDefinition X1_15 = CreateBit("input.x1_15", "X1.15", "CB gốc bàn xoay");

        // --- Word 2: X2.00 - X2.15 ---
        public static readonly PlcTagDefinition X2_00 = CreateBit("input.x2_00", "X2.00", "CB gốc cụm nâng");
        public static readonly PlcTagDefinition X2_01 = CreateBit("input.x2_01", "X2.01", "CB Giới hạn trên cụm nâng");
        public static readonly PlcTagDefinition X2_02 = CreateBit("input.x2_02", "X2.02", "CB giới hạn dưới cụm nâng");
        public static readonly PlcTagDefinition X2_03 = CreateBit("input.x2_03", "X2.03", "CB XL Cụm đầu vào Xoay 0");
        public static readonly PlcTagDefinition X2_04 = CreateBit("input.x2_04", "X2.04", "CB XL Cụm đầu vào Xoay 90");
        public static readonly PlcTagDefinition X2_05 = CreateBit("input.x2_05", "X2.05", "CB XL kẹp đầu vào");
        public static readonly PlcTagDefinition X2_06 = CreateBit("input.x2_06", "X2.06", "CB XL Mở kẹp đầu vào");
        public static readonly PlcTagDefinition X2_07 = CreateBit("input.x2_07", "X2.07", "CB XL check hàng đầu vào");
        public static readonly PlcTagDefinition X2_08 = CreateBit("input.x2_08", "X2.08", "Tín hiệu chạy bàn xoay");
        public static readonly PlcTagDefinition X2_09 = CreateBit("input.x2_09", "X2.09", "Tín hiệu lỗi bàn xoay");
        public static readonly PlcTagDefinition X2_10 = CreateBit("input.x2_10", "X2.10", "Tín hiệu chạy cụm nâng");
        public static readonly PlcTagDefinition X2_11 = CreateBit("input.x2_11", "X2.11", "Tín hiệu báo lỗi cụm nâng");
        public static readonly PlcTagDefinition X2_12 = CreateBit("input.x2_12", "X2.12", "Lỗi trục X");
        public static readonly PlcTagDefinition X2_13 = CreateBit("input.x2_13", "X2.13", "Lỗi trục Z");
        public static readonly PlcTagDefinition X2_14 = CreateBit("input.x2_14", "X2.14", "CB Check hàng trước Hope");
        public static readonly PlcTagDefinition X2_15 = CreateBit("input.x2_15", "X2.15", "X2.15");

        // --- Word 3: X3.00 - X3.15 ---
        public static readonly PlcTagDefinition X3_00 = CreateBit("input.x3_00", "X3.00", "CB Tay Rodal Xoay 0");
        public static readonly PlcTagDefinition X3_01 = CreateBit("input.x3_01", "X3.01", "CB Tay Rodal Xoay 180");
        public static readonly PlcTagDefinition X3_02 = CreateBit("input.x3_02", "X3.02", "CB Check phôi Tay Rodal 1");
        public static readonly PlcTagDefinition X3_03 = CreateBit("input.x3_03", "X3.03", "CB Check phôi Tay Rodal 2");
        public static readonly PlcTagDefinition X3_04 = CreateBit("input.x3_04", "X3.04", "X3.4");
        public static readonly PlcTagDefinition X3_05 = CreateBit("input.x3_05", "X3.05", "X3.5");
        public static readonly PlcTagDefinition X3_06 = CreateBit("input.x3_06", "X3.06", "X3.6");
        public static readonly PlcTagDefinition X3_07 = CreateBit("input.x3_07", "X3.07", "X3.7");
        public static readonly PlcTagDefinition X3_08 = CreateBit("input.x3_08", "X3.08", "CB XL Xoay 0 Output");
        public static readonly PlcTagDefinition X3_09 = CreateBit("input.x3_09", "X3.09", "CB XL Xoay 90 Output");
        public static readonly PlcTagDefinition X3_10 = CreateBit("input.x3_10", "X3.10", "CB XL Chuyển hàng Y ở trong");
        public static readonly PlcTagDefinition X3_11 = CreateBit("input.x3_11", "X3.11", "CB XL Chuyển hàng Y ở ngoài");
        public static readonly PlcTagDefinition X3_12 = CreateBit("input.x3_12", "X3.12", "CB XL Chuyển hàng X ở trong");
        public static readonly PlcTagDefinition X3_13 = CreateBit("input.x3_13", "X3.13", "CB XL Chuyển hàng X ở ngoài");
        public static readonly PlcTagDefinition X3_14 = CreateBit("input.x3_14", "X3.14", "CB XL Kẹp Trước Hope");
        public static readonly PlcTagDefinition X3_15 = CreateBit("input.x3_15", "X3.15", "CB XL Mở Kẹp Trước Hope");

        // --- Word 4: X4.00 - X4.15 ---
        public static readonly PlcTagDefinition X4_00 = CreateBit("input.x4_00", "X4.00", "Máy tiện OP1 Báo Chạy");
        public static readonly PlcTagDefinition X4_01 = CreateBit("input.x4_01", "X4.01", "Máy tiện OP1 Hoàn thành");
        public static readonly PlcTagDefinition X4_02 = CreateBit("input.x4_02", "X4.02", "Máy tiện OP1 báo Lỗi");
        public static readonly PlcTagDefinition X4_03 = CreateBit("input.x4_03", "X4.03", "Máy tiện Op1 nút dừng khẩn");
        public static readonly PlcTagDefinition X4_04 = CreateBit("input.x4_04", "X4.04", "Máy tiện OP1 Kẹp chấu cặp");
        public static readonly PlcTagDefinition X4_05 = CreateBit("input.x4_05", "X4.05", "Máy tiện OP1 Mở kẹp chấu cặp");
        public static readonly PlcTagDefinition X4_06 = CreateBit("input.x4_06", "X4.06", "Máy tiện Op1 mở cửa");
        public static readonly PlcTagDefinition X4_07 = CreateBit("input.x4_07", "X4.07", "X4.7");
        public static readonly PlcTagDefinition X4_08 = CreateBit("input.x4_08", "X4.08", "Máy tiện OP2 Báo Chạy");
        public static readonly PlcTagDefinition X4_09 = CreateBit("input.x4_09", "X4.09", "Máy tiện OP2 Hoàn thành");
        public static readonly PlcTagDefinition X4_10 = CreateBit("input.x4_10", "X4.10", "Máy tiện OP2 báo Lỗi");
        public static readonly PlcTagDefinition X4_11 = CreateBit("input.x4_11", "X4.11", "Máy tiện Op2 nút dừng khẩn");
        public static readonly PlcTagDefinition X4_12 = CreateBit("input.x4_12", "X4.12", "Máy tiện OP2 Kẹp chấu cặp");
        public static readonly PlcTagDefinition X4_13 = CreateBit("input.x4_13", "X4.13", "Máy tiện OP2 Mở kẹp chấu cặp");
        public static readonly PlcTagDefinition X4_14 = CreateBit("input.x4_14", "X4.14", "Máy tiện Op2 mở cửa");
        public static readonly PlcTagDefinition X4_15 = CreateBit("input.x4_15", "X4.15", "X4.15");
    }

    public static class Outputs
    {
        public static readonly PlcTagDefinition Y0_00 = CreateBit("output.y0_00", "Y0.00", "Y0.00");
        public static readonly PlcTagDefinition Y0_01 = CreateBit("output.y0_01", "Y0.01", "Y0.01");
        public static readonly PlcTagDefinition Y0_02 = CreateBit("output.y0_02", "Y0.02", "Y0.02");
        public static readonly PlcTagDefinition Y0_03 = CreateBit("output.y0_03", "Y0.03", "Y0.03");
        public static readonly PlcTagDefinition Y0_04 = CreateBit("output.y0_04", "Y0.04", "Y0.04");
        public static readonly PlcTagDefinition Y0_05 = CreateBit("output.y0_05", "Y0.05", "Y0.05");
        public static readonly PlcTagDefinition Y0_06 = CreateBit("output.y0_06", "Y0.06", "Y0.06");
        public static readonly PlcTagDefinition Y0_07 = CreateBit("output.y0_07", "Y0.07", "Y0.07");
        public static readonly PlcTagDefinition Y0_08 = CreateBit("output.y0_08", "Y0.08", "Y0.08");
        public static readonly PlcTagDefinition Y0_09AxisXOn = CreateBit("output.y0_09", "Y0.09", "SERVO ON TRỤC X");
        public static readonly PlcTagDefinition Y0_10AxisYOn = CreateBit("output.y0_10", "Y0.10", "SERVO ON TRỤC Y");
        public static readonly PlcTagDefinition Y0_11AxisZOn = CreateBit("output.y0_11", "Y0.11", "SERVO ON TRỤC Z");
        public static readonly PlcTagDefinition Y0_12 = CreateBit("output.y0_12", "Y0.12", "Y0.12");
        public static readonly PlcTagDefinition Y0_13 = CreateBit("output.y0_13", "Y0.13", "Y0.13");
        public static readonly PlcTagDefinition Y0_14 = CreateBit("output.y0_14", "Y0.14", "Y0.14");
        public static readonly PlcTagDefinition Y0_15 = CreateBit("output.y0_15", "Y0.15", "Y0.15");

        public static readonly PlcTagDefinition Y1_00LightRed = CreateBit("output.y1_00", "Y1.00", "ĐÈN ĐỎ");
        public static readonly PlcTagDefinition Y1_01LightYellow = CreateBit("output.y1_01", "Y1.01", "ĐÈN VÀNG");
        public static readonly PlcTagDefinition Y1_02LightGreen = CreateBit("output.y1_02", "Y1.02", "ĐÈN XANH");
        public static readonly PlcTagDefinition Y1_03LightBuzzer = CreateBit("output.y1_03", "Y1.03", "CÒI BÁO");
        public static readonly PlcTagDefinition Y1_04XlCenter1In = CreateBit("output.y1_04", "Y1.04", "XL KẸP XE 2");
        public static readonly PlcTagDefinition Y1_05XlCenter1Out = CreateBit("output.y1_05", "Y1.05", "XL MỞ KẸP XE 2");
        public static readonly PlcTagDefinition Y1_06XlCenter2In = CreateBit("output.y1_06", "Y1.06", "XL KẸP XE 1");
        public static readonly PlcTagDefinition Y1_07XlCenter2Out = CreateBit("output.y1_07", "Y1.07", "XL MỞ KẸP XE 1");
        public static readonly PlcTagDefinition Y1_08XlClampPartIn = CreateBit("output.y1_08", "Y1.08", "XL TOOL KẸP");
        public static readonly PlcTagDefinition Y1_09XlClampPartOut = CreateBit("output.y1_09", "Y1.09", "XL TOOL MỞ KẸP");
        public static readonly PlcTagDefinition Y1_10XlRotary0 = CreateBit("output.y1_10", "Y1.10", "XL XOAY 0");
        public static readonly PlcTagDefinition Y1_11XlRotary90 = CreateBit("output.y1_11", "Y1.11", "XL XOAY 90");
        public static readonly PlcTagDefinition Y1_12BtLightStop = CreateBit("output.y1_12", "Y1.12", "ĐÈN NÚT DỪNG");
        public static readonly PlcTagDefinition Y1_13BtLightStart = CreateBit("output.y1_13", "Y1.13", "ĐÈN NÚT CHẠY");
        public static readonly PlcTagDefinition Y1_14BtLightReset = CreateBit("output.y1_14", "Y1.14", "ĐÈN NÚT XÓA LỖI");
        public static readonly PlcTagDefinition Y1_15 = CreateBit("output.y1_15", "Y1.15", "ĐÈN NÚT VỀ GỐC");
    }

    public static class DataAutos
    {
        // --- Shelf 1 summary ---
        public static readonly PlcTagDefinition Shelf1ProductCount = CreateWord("data.shelf_1_product_count", "D5500", PlcTagDataType.Int16, "Số lượng hàng trên kệ 1");
        public static readonly PlcTagDefinition Shelf1OrderCount = CreateWord("data.shelf_1_order_count", "D5501", PlcTagDataType.Int16, "Số lượng order trên kệ 1");

        // --- Order Line 1 ---
        public static readonly PlcTagDefinition OrderLine1Code = CreateWord("data.order_line_1_code", "D5505", PlcTagDataType.String, "OrderId line 1", 30);
        public static readonly PlcTagDefinition OrderLine1ModelId = CreateWord("data.order_line_1_model_id", "D5520", PlcTagDataType.String, "ModelId line 1", 30);
        public static readonly PlcTagDefinition OrderLine1ItemCode = OrderLine1ModelId;
        public static readonly PlcTagDefinition OrderLine1Quantity = CreateWord("data.order_line_1_quantity", "D5535", PlcTagDataType.Int16, "Số lượng hàng trong order line 1");
        public static readonly PlcTagDefinition OrderLine1JigType = CreateWord("data.order_line_1_jig_type", "D5536", PlcTagDataType.Int16, "Loại jig kẹp line 1");
        public static readonly PlcTagDefinition OrderLine1StartPosition = CreateWord("data.order_line_1_start_position", "D5537", PlcTagDataType.Int16, "Vị trí sản phẩm đầu tiên của order trong tray line 1");
        public static readonly PlcTagDefinition OrderLine1TrayIndex = CreateWord("data.order_line_1_tray_index", "D5538", PlcTagDataType.Int16, "Thứ tự tray trong kệ line 1");
        public static readonly PlcTagDefinition OrderLine1TrayType = CreateWord("data.order_line_1_tray_type", "D5539", PlcTagDataType.Int16, "Loại tray line 1");
        public static readonly PlcTagDefinition OrderLine1Sequence = CreateWord("data.order_line_1_sequence", "D5540", PlcTagDataType.Int16, "Số thứ tự order trong kệ line 1");
        // Cải tiến theo yêu cầu mr.Tùng ngày 27/04/2026: Ẩn và ngừng ghi các điểm check gốc robot
        /*
        public static readonly PlcTagDefinition OrderLine1CheckPoint1X = CreateWord("data.order_line_1_check_point_1_x", "D5547", PlcTagDataType.Float, "Tọa độ X góc check 1 line 1");
        public static readonly PlcTagDefinition OrderLine1CheckPoint1Y = CreateWord("data.order_line_1_check_point_1_y", "D5549", PlcTagDataType.Float, "Tọa độ Y góc check 1 line 1");
        public static readonly PlcTagDefinition OrderLine1CheckPoint1Z = CreateWord("data.order_line_1_check_point_1_z", "D5551", PlcTagDataType.Float, "Tọa độ Z góc check 1 line 1");
        */
        public static readonly PlcTagDefinition OrderLine1PartHoverHeight = CreateWord("data.order_line_1_part_hover_height", "D5553", PlcTagDataType.Float, "Độ cao trên Jig line 1");
        public static readonly PlcTagDefinition OrderLine1JigCenterOffset = CreateWord("data.order_line_1_jig_center_offset", "D5558", PlcTagDataType.Float, "Ofset Tâm Jig line 1");
        public static readonly PlcTagDefinition OrderLine1JigDepthOffset = CreateWord("data.order_line_1_jig_depth_offset", "D5560", PlcTagDataType.Float, "Ofset độ cao âm xuống Jig line 1");
        public static readonly PlcTagDefinition OrderLine1PickedCount = CreateWord("data.order_line_1_picked_count", "D5562", PlcTagDataType.Int16, "Số thứ tự của con hàng đã gắp trong order line 1");
        public static readonly PlcTagDefinition OrderLine1DiameterOp1 = CreateWord("data.order_line_1_diameter_op1", "D5563", PlcTagDataType.Float, "Đường kính Op1 line 1");
        public static readonly PlcTagDefinition OrderLine1IsLoading = CreateBit("data.order_line_1_is_loading", "D5555.0", "Đang load dữ liệu order line 1");
        public static readonly PlcTagDefinition OrderLine1ProductionResultAcknowledged = CreateBit("data.order_line_1_production_result_acknowledged", "D5555.1", "Đã ghi nhận kết quả sản xuất order line 1");
        public static readonly PlcTagDefinition OrderLine1PausedByPc = CreateBit("data.order_line_1_paused_by_pc", "D5555.2", "Tạm dừng Line 1 - kệ 1");
        public static readonly PlcTagDefinition OrderLine1ClearRequestedByPc = CreateBit("data.order_line_1_clear_requested_by_pc", "D5555.3", "Hủy Order Line 1 - kệ 1");
        public static readonly PlcTagDefinition OrderLine1ShelfOrdersCompleted = CreateBit("data.order_line_1_shelf_orders_completed", "D5555.4", "Đã Hoàn thành Order trên kệ line 1");
        public static readonly PlcTagDefinition CurrentOrderRunStarted = CreateBit("data.current_order_run_started", "D5556.0", "Cờ báo chạy");
        public static readonly PlcTagDefinition CurrentOrderLoadCompleted = CreateBit("data.current_order_load_completed", "D5556.1", "Cờ báo đã load xong dữ liệu order");
        public static readonly PlcTagDefinition CurrentOrderCompleted = CreateBit("data.current_order_completed", "D5556.2", "Cờ báo chạy xong order hiện tại");
        public static readonly PlcTagDefinition OrderLine1CurrentPickIndex = CreateWord("data.order_line_1_current_pick_index", "D5557", PlcTagDataType.Int16, "Số thứ tự của con hàng đang gắp trong order line 1");
        public static readonly PlcTagDefinition RanQuantityOrderLine1 = OrderLine1PickedCount;

        // --- Shelf 2 summary ---
        public static readonly PlcTagDefinition Shelf2ProductCount = CreateWord("data.shelf_2_product_count", "D5570", PlcTagDataType.Int16, "Số lượng hàng trên kệ 2");
        public static readonly PlcTagDefinition Shelf2OrderCount = CreateWord("data.shelf_2_order_count", "D5571", PlcTagDataType.Int16, "Số lượng order trên kệ 2");

        // --- Order Line 2 ---
        public static readonly PlcTagDefinition OrderLine2Code = CreateWord("data.order_line_2_code", "D5575", PlcTagDataType.String, "OrderId line 2", 30);
        public static readonly PlcTagDefinition OrderLine2ModelId = CreateWord("data.order_line_2_model_id", "D5590", PlcTagDataType.String, "ModelId line 2", 30);
        public static readonly PlcTagDefinition OrderLine2ItemCode = OrderLine2ModelId;
        public static readonly PlcTagDefinition OrderLine2Quantity = CreateWord("data.order_line_2_quantity", "D5605", PlcTagDataType.Int16, "Số lượng hàng trong order line 2");
        public static readonly PlcTagDefinition OrderLine2JigType = CreateWord("data.order_line_2_jig_type", "D5606", PlcTagDataType.Int16, "Loại jig kẹp line 2");
        public static readonly PlcTagDefinition OrderLine2StartPosition = CreateWord("data.order_line_2_start_position", "D5607", PlcTagDataType.Int16, "Vị trí sản phẩm đầu tiên của order trong tray line 2");
        public static readonly PlcTagDefinition OrderLine2TrayIndex = CreateWord("data.order_line_2_tray_index", "D5608", PlcTagDataType.Int16, "Thứ tự tray trong kệ line 2");
        public static readonly PlcTagDefinition OrderLine2TrayType = CreateWord("data.order_line_2_tray_type", "D5609", PlcTagDataType.Int16, "Loại tray line 2");
        public static readonly PlcTagDefinition OrderLine2Sequence = CreateWord("data.order_line_2_sequence", "D5610", PlcTagDataType.Int16, "Số thứ tự order trong kệ line 2");
        // Cải tiến theo yêu cầu mr.Tùng ngày 27/04/2026: Ẩn và ngừng ghi các điểm check gốc robot
        /*
        public static readonly PlcTagDefinition OrderLine2CheckPoint1X = CreateWord("data.order_line_2_check_point_1_x", "D5617", PlcTagDataType.Float, "Tọa độ X góc check 1 line 2");
        public static readonly PlcTagDefinition OrderLine2CheckPoint1Y = CreateWord("data.order_line_2_check_point_1_y", "D5619", PlcTagDataType.Float, "Tọa độ Y góc check 1 line 2");
        public static readonly PlcTagDefinition OrderLine2CheckPoint1Z = CreateWord("data.order_line_2_check_point_1_z", "D5621", PlcTagDataType.Float, "Tọa độ Z góc check 1 line 2");
        */
        public static readonly PlcTagDefinition OrderLine2PartHoverHeight = CreateWord("data.order_line_2_part_hover_height", "D5623", PlcTagDataType.Float, "Độ cao trên Jig line 2");
        public static readonly PlcTagDefinition OrderLine2JigCenterOffset = CreateWord("data.order_line_2_jig_center_offset", "D5628", PlcTagDataType.Float, "Ofset Tâm Jig line 2");
        public static readonly PlcTagDefinition OrderLine2JigDepthOffset = CreateWord("data.order_line_2_jig_depth_offset", "D5630", PlcTagDataType.Float, "Ofset độ cao âm xuống Jig line 2");
        public static readonly PlcTagDefinition OrderLine2PickedCount = CreateWord("data.order_line_2_picked_count", "D5632", PlcTagDataType.Int16, "Số thứ tự của con hàng đã gắp trong order line 2");
        public static readonly PlcTagDefinition OrderLine2DiameterOp1 = CreateWord("data.order_line_2_diameter_op1", "D5633", PlcTagDataType.Float, "Đường kính Op1 line 2");
        public static readonly PlcTagDefinition OrderLine2IsLoading = CreateBit("data.order_line_2_is_loading", "D5625.0", "Đang load dữ liệu order line 2");
        public static readonly PlcTagDefinition OrderLine2ProductionResultAcknowledged = CreateBit("data.order_line_2_production_result_acknowledged", "D5625.1", "Đã ghi nhận kết quả sản xuất order line 2");
        public static readonly PlcTagDefinition OrderLine2PausedByPc = CreateBit("data.order_line_2_paused_by_pc", "D5625.2", "Tạm dừng Line 2 - kệ 2");
        public static readonly PlcTagDefinition OrderLine2ClearRequestedByPc = CreateBit("data.order_line_2_clear_requested_by_pc", "D5625.3", "Hủy Order Line 2 - kệ 2");
        public static readonly PlcTagDefinition OrderLine2ShelfOrdersCompleted = CreateBit("data.order_line_2_shelf_orders_completed", "D5625.4", "Đã Hoàn thành Order trên kệ line 2");
        public static readonly PlcTagDefinition CurrentOrderRunStartedLine2 = CreateBit("data.current_order_run_started_line2", "D5626.0", "Cờ báo chạy line 2");
        public static readonly PlcTagDefinition CurrentOrderLoadCompletedLine2 = CreateBit("data.current_order_load_completed_line2", "D5626.1", "Cờ báo đã load xong dữ liệu order line 2");
        public static readonly PlcTagDefinition CurrentOrderCompletedLine2 = CreateBit("data.current_order_completed_line2", "D5626.2", "Cờ báo chạy xong order hiện tại line 2");
        public static readonly PlcTagDefinition OrderLine2CurrentPickIndex = CreateWord("data.order_line_2_current_pick_index", "D5627", PlcTagDataType.Int16, "Số thứ tự của con hàng đang gắp trong order line 2");
        public static readonly PlcTagDefinition RanQuantityOrderLine2 = OrderLine2PickedCount;

        // --- Control ---
        public static readonly PlcTagDefinition Clock1s = CreateWord("data.clock_1s", "D5640", PlcTagDataType.Int16, "Clock 1s");
    }

    public static class RobotTest
    {
        public static readonly PlcTagDefinition JigProductHeight = CreateWord("robot_test.jig_product_height", "D5672", PlcTagDataType.Float, "Test độ cao trên Jig");
        public static readonly PlcTagDefinition JigCenterOffset = CreateWord("robot_test.jig_center_offset", "D5674", PlcTagDataType.Float, "Test Ofset Tâm Jig");
        public static readonly PlcTagDefinition JigDepthOffset = CreateWord("robot_test.jig_depth_offset", "D5676", PlcTagDataType.Float, "Test Ofset độ cao âm xuống Jig");
        public static readonly PlcTagDefinition DiameterOp1 = CreateWord("robot_test.diameter_op1", "D5678", PlcTagDataType.Float, "Test đường kính Op1");
        public static readonly PlcTagDefinition TrayType = CreateWord("robot_test.tray_type", "D5681", PlcTagDataType.Int16, "Test Loại Tray");
        public static readonly PlcTagDefinition RunLine1 = CreateBit("robot_test.run_line1", "M2070", "Chạy gấp hàng Line 1");
        public static readonly PlcTagDefinition RunLine2 = CreateBit("robot_test.run_line2", "M2071", "Chạy gấp hàng Line 2");
        public static readonly PlcTagDefinition CancelPickLine1 = CreateBit("robot_test.cancel_pick_line1", "M2072", "Hủy gắp hàng Line 1");
        public static readonly PlcTagDefinition CancelPickLine2 = CreateBit("robot_test.cancel_pick_line2", "M2073", "Hủy gắp hàng Line 2");
    }

    public static class DataTrayCart
    {
        public static readonly PlcTagDefinition Cart1SmallTrayPos1X = CreateWord("data.cart_1_small_tray_pos_1_x", "D21000", PlcTagDataType.Float, "Vị trí trục X xe 1 khay nhỏ ở vị trí 1");
        public static readonly PlcTagDefinition Cart1SmallTrayPos1Y = CreateWord("data.cart_1_small_tray_pos_1_y", "D21002", PlcTagDataType.Float, "Vị trí trục Y xe 1 khay nhỏ ở vị trí 1");
        public static readonly PlcTagDefinition Cart1SmallTrayPos1Z = CreateWord("data.cart_1_small_tray_pos_1_z", "D21004", PlcTagDataType.Float, "Vị trí trục Z xe 1 khay nhỏ ở vị trí 1");
        public static readonly PlcTagDefinition Cart1SmallTrayPos2X = CreateWord("data.cart_1_small_tray_pos_2_x", "D21006", PlcTagDataType.Float, "Vị trí trục X xe 1 khay nhỏ ở vị trí 2");
        public static readonly PlcTagDefinition Cart1SmallTrayPos2Y = CreateWord("data.cart_1_small_tray_pos_2_y", "D21008", PlcTagDataType.Float, "Vị trí trục Y xe 1 khay nhỏ ở vị trí 2");
        public static readonly PlcTagDefinition Cart1SmallTrayPos2Z = CreateWord("data.cart_1_small_tray_pos_2_z", "D21010", PlcTagDataType.Float, "Vị trí trục Z xe 1 khay nhỏ ở vị trí 2");

        public static readonly PlcTagDefinition Cart1LargeTrayPos1X = CreateWord("data.cart_1_large_tray_pos_1_x", "D21012", PlcTagDataType.Float, "Vị trí trục X xe 1 khay lớn ở vị trí 1");
        public static readonly PlcTagDefinition Cart1LargeTrayPos1Y = CreateWord("data.cart_1_large_tray_pos_1_y", "D21014", PlcTagDataType.Float, "Vị trí trục Y xe 1 khay lớn ở vị trí 1");
        public static readonly PlcTagDefinition Cart1LargeTrayPos1Z = CreateWord("data.cart_1_large_tray_pos_1_z", "D21016", PlcTagDataType.Float, "Vị trí trục Z xe 1 khay lớn ở vị trí 1");
        public static readonly PlcTagDefinition Cart1LargeTrayPos2X = CreateWord("data.cart_1_large_tray_pos_2_x", "D21018", PlcTagDataType.Float, "Vị trí trục X xe 1 khay lớn ở vị trí 2");
        public static readonly PlcTagDefinition Cart1LargeTrayPos2Y = CreateWord("data.cart_1_large_tray_pos_2_y", "D21020", PlcTagDataType.Float, "Vị trí trục Y xe 1 khay lớn ở vị trí 2");
        public static readonly PlcTagDefinition Cart1LargeTrayPos2Z = CreateWord("data.cart_1_large_tray_pos_2_z", "D21022", PlcTagDataType.Float, "Vị trí trục Z xe 1 khay lớn ở vị trí 2");

        public static readonly PlcTagDefinition Cart2SmallTrayPos1X = CreateWord("data.cart_2_small_tray_pos_1_x", "D21024", PlcTagDataType.Float, "Vị trí trục X xe 2 khay nhỏ ở vị trí 1");
        public static readonly PlcTagDefinition Cart2SmallTrayPos1Y = CreateWord("data.cart_2_small_tray_pos_1_y", "D21026", PlcTagDataType.Float, "Vị trí trục Y xe 2 khay nhỏ ở vị trí 1");
        public static readonly PlcTagDefinition Cart2SmallTrayPos1Z = CreateWord("data.cart_2_small_tray_pos_1_z", "D21028", PlcTagDataType.Float, "Vị trí trục Z xe 2 khay nhỏ ở vị trí 1");
        public static readonly PlcTagDefinition Cart2SmallTrayPos2X = CreateWord("data.cart_2_small_tray_pos_2_x", "D21030", PlcTagDataType.Float, "Vị trí trục X xe 2 khay nhỏ ở vị trí 2");
        public static readonly PlcTagDefinition Cart2SmallTrayPos2Y = CreateWord("data.cart_2_small_tray_pos_2_y", "D21032", PlcTagDataType.Float, "Vị trí trục Y xe 2 khay nhỏ ở vị trí 2");
        public static readonly PlcTagDefinition Cart2SmallTrayPos2Z = CreateWord("data.cart_2_small_tray_pos_2_z", "D21034", PlcTagDataType.Float, "Vị trí trục Z xe 2 khay nhỏ ở vị trí 2");

        public static readonly PlcTagDefinition Cart2LargeTrayPos1X = CreateWord("data.cart_2_large_tray_pos_1_x", "D21036", PlcTagDataType.Float, "Vị trí trục X xe 2 khay lớn ở vị trí 1");
        public static readonly PlcTagDefinition Cart2LargeTrayPos1Y = CreateWord("data.cart_2_large_tray_pos_1_y", "D21038", PlcTagDataType.Float, "Vị trí trục Y xe 2 khay lớn ở vị trí 1");
        public static readonly PlcTagDefinition Cart2LargeTrayPos1Z = CreateWord("data.cart_2_large_tray_pos_1_z", "D21040", PlcTagDataType.Float, "Vị trí trục Z xe 2 khay lớn ở vị trí 1");
        public static readonly PlcTagDefinition Cart2LargeTrayPos2X = CreateWord("data.cart_2_large_tray_pos_2_x", "D21042", PlcTagDataType.Float, "Vị trí trục X xe 2 khay lớn ở vị trí 2");
        public static readonly PlcTagDefinition Cart2LargeTrayPos2Y = CreateWord("data.cart_2_large_tray_pos_2_y", "D21044", PlcTagDataType.Float, "Vị trí trục Y xe 2 khay lớn ở vị trí 2");
        public static readonly PlcTagDefinition Cart2LargeTrayPos2Z = CreateWord("data.cart_2_large_tray_pos_2_z", "D21046", PlcTagDataType.Float, "Vị trí trục Z xe 2 khay lớn ở vị trí 2");

        // --- Thông số Tray Nhỏ ---
        public static readonly PlcTagDefinition SmallTrayColumnSpacing = CreateWord("data.small_tray_column_spacing", "D21048", PlcTagDataType.Float, "Khoảng cách 2 cột trên tray nhỏ");
        public static readonly PlcTagDefinition SmallTrayRowSpacing = CreateWord("data.small_tray_row_spacing", "D21050", PlcTagDataType.Float, "Khoảng cách 2 hàng trên tray nhỏ");
        public static readonly PlcTagDefinition SmallTrayJigHeight = CreateWord("data.small_tray_jig_height", "D21052", PlcTagDataType.Float, "Độ cao Jig Tray Nhỏ");
        public static readonly PlcTagDefinition SmallTrayColumns = CreateWord("data.small_tray_columns", "D21054", PlcTagDataType.Int16, "Số cột Tray nhỏ");
        public static readonly PlcTagDefinition SmallTrayRows = CreateWord("data.small_tray_rows", "D21056", PlcTagDataType.Int16, "Số hàng Tray nhỏ");
        // --- Thông số Tray Lớn ---
        public static readonly PlcTagDefinition LargeTrayColumnSpacing = CreateWord("data.large_tray_column_spacing", "D21058", PlcTagDataType.Float, "Khoảng cách 2 cột trên tray lớn");
        public static readonly PlcTagDefinition LargeTrayRowSpacing = CreateWord("data.large_tray_row_spacing", "D21060", PlcTagDataType.Float, "Khoảng cách 2 hàng trên tray lớn");
        public static readonly PlcTagDefinition LargeTrayColumns = CreateWord("data.large_tray_columns", "D21062", PlcTagDataType.Int16, "Số cột Tray Lớn");
        public static readonly PlcTagDefinition LargeTrayRows = CreateWord("data.large_tray_rows", "D21064", PlcTagDataType.Int16, "Số hàng Tray Lớn");
        public static readonly PlcTagDefinition LargeTrayJigHeight = CreateWord("data.large_tray_jig_height", "D21066", PlcTagDataType.Float, "Độ cao Jig Tray Lớn");
    }

    public static class DataMachine
    {
        public static readonly PlcTagDefinition AxisXSpeedLimit = CreateWord("data.axis_x_speed_limit", "D21070", PlcTagDataType.Float, "Giới hạn tốc độ trục X");
        public static readonly PlcTagDefinition AxisYSpeedLimit = CreateWord("data.axis_y_speed_limit", "D21072", PlcTagDataType.Float, "Giới hạn tốc độ trục Y");
        public static readonly PlcTagDefinition AxisZSpeedLimit = CreateWord("data.axis_z_speed_limit", "D21074", PlcTagDataType.Float, "Giới hạn tốc độ trục Z");
        public static readonly PlcTagDefinition AxisXPositiveLimit = CreateWord("data.axis_x_positive_limit", "D21076", PlcTagDataType.Float, "Giới hạn trục X+");
        public static readonly PlcTagDefinition AxisXNegativeLimit = CreateWord("data.axis_x_negative_limit", "D21078", PlcTagDataType.Float, "Giới hạn trục X-");
        public static readonly PlcTagDefinition AxisYPositiveLimit = CreateWord("data.axis_y_positive_limit", "D21080", PlcTagDataType.Float, "Giới hạn trục Y+");
        public static readonly PlcTagDefinition AxisYNegativeLimit = CreateWord("data.axis_y_negative_limit", "D21082", PlcTagDataType.Float, "Giới hạn trục Y-");
        public static readonly PlcTagDefinition AxisZPositiveLimit = CreateWord("data.axis_z_positive_limit", "D21084", PlcTagDataType.Float, "Giới hạn trục Z+");
        public static readonly PlcTagDefinition AxisZNegativeLimit = CreateWord("data.axis_z_negative_limit", "D21086", PlcTagDataType.Float, "Giới hạn trục Z-");
        public static readonly PlcTagDefinition AxisZPickDownSpeed = CreateWord("data.axis_z_pick_down_speed", "D21088", PlcTagDataType.Float, "Tốc độ xuống gắp hàng trục Z");
        public static readonly PlcTagDefinition AxisZPickSpeed = CreateWord("data.axis_z_pick_speed", "D21090", PlcTagDataType.Float, "Tốc độ lấy hàng trục Z");
        public static readonly PlcTagDefinition AxisZDropSpeed = CreateWord("data.axis_z_drop_speed", "D21092", PlcTagDataType.Float, "Tốc độ thả hàng trục Z");
        public static readonly PlcTagDefinition AxisXAutoSpeed = CreateWord("data.axis_x_auto_speed", "D21094", PlcTagDataType.Float, "Tốc độ chạy tự động trục X");
        public static readonly PlcTagDefinition AxisYAutoSpeed = CreateWord("data.axis_y_auto_speed", "D21096", PlcTagDataType.Float, "Tốc độ chạy tự động trục Y");
        public static readonly PlcTagDefinition AxisZAutoSpeed = CreateWord("data.axis_z_auto_speed", "D21098", PlcTagDataType.Float, "Tốc độ chạy tự động trục Z");
        public static readonly PlcTagDefinition WaitPickPositionX = CreateWord("data.wait_pick_position_x", "D21100", PlcTagDataType.Float, "Tọa độ X vị trí chờ gắp hàng");
        public static readonly PlcTagDefinition WaitPickPositionY = CreateWord("data.wait_pick_position_y", "D21102", PlcTagDataType.Float, "Tọa độ Y vị trí chờ gắp hàng");
        public static readonly PlcTagDefinition WaitPickPositionZ = CreateWord("data.wait_pick_position_z", "D21104", PlcTagDataType.Float, "Tọa độ Z vị trí chờ gắp hàng");
        public static readonly PlcTagDefinition SafeRotatePositionZ = CreateWord("data.safe_rotate_position_z", "D21106", PlcTagDataType.Float, "Tọa độ Z an toàn xoay trên");
        public static readonly PlcTagDefinition SafeRotatePositionX = CreateWord("data.safe_rotate_position_x", "D21140", PlcTagDataType.Float, "Tọa độ An Toàn xoay X");
        public static readonly PlcTagDefinition SafeRotatePositionZLower = CreateWord("data.safe_rotate_position_z_lower", "D21142", PlcTagDataType.Float, "Tọa độ An toàn xoay Z dưới");
        public static readonly PlcTagDefinition PlaceProductLine1X = CreateWord("data.place_product_line1_x", "D21144", PlcTagDataType.Float, "Tọa độ X đặt sản phẩm Line 1");
        public static readonly PlcTagDefinition PlaceProductLine1Y = CreateWord("data.place_product_line1_y", "D21146", PlcTagDataType.Float, "Tọa độ Y đặt sản phẩm Line 1");
        public static readonly PlcTagDefinition PlaceProductLine1Z = CreateWord("data.place_product_line1_z", "D21148", PlcTagDataType.Float, "Tọa độ Z đặt sản phẩm Line 1");
        public static readonly PlcTagDefinition PlaceProductLine2X = CreateWord("data.place_product_line2_x", "D21150", PlcTagDataType.Float, "Tọa độ X đặt sản phẩm Line 2");
        public static readonly PlcTagDefinition PlaceProductLine2Y = CreateWord("data.place_product_line2_y", "D21152", PlcTagDataType.Float, "Tọa độ Y đặt sản phẩm Line 2");
        public static readonly PlcTagDefinition PlaceProductLine2Z = CreateWord("data.place_product_line2_z", "D21154", PlcTagDataType.Float, "Tọa độ Z đặt sản phẩm Line 2");
        public static readonly PlcTagDefinition MasterShaftOp1Diameter = CreateWord("data.master_shaft_op1_diameter", "D21156", PlcTagDataType.Float, "Đường kính OP1 trục Master");
        public static readonly PlcTagDefinition AxisXTorqueUpperLimit = CreateWord("data.axis_x_torque_upper_limit", "D21158", PlcTagDataType.Int16, "Giới hạn trên Momen trục X");
        public static readonly PlcTagDefinition AxisXTorqueLowerLimit = CreateWord("data.axis_x_torque_lower_limit", "D21159", PlcTagDataType.Int16, "Giới hạn dưới Momen trục X");
        public static readonly PlcTagDefinition AxisYTorqueUpperLimit = CreateWord("data.axis_y_torque_upper_limit", "D21160", PlcTagDataType.Int16, "Giới hạn trên Momen trục Y");
        public static readonly PlcTagDefinition AxisYTorqueLowerLimit = CreateWord("data.axis_y_torque_lower_limit", "D21161", PlcTagDataType.Int16, "Giới hạn dưới Momen trục Y");
        public static readonly PlcTagDefinition AxisZTorqueUpperLimit = CreateWord("data.axis_z_torque_upper_limit", "D21162", PlcTagDataType.Int16, "Giới hạn trên Momen trục Z");
        public static readonly PlcTagDefinition AxisZTorqueLowerLimit = CreateWord("data.axis_z_torque_lower_limit", "D21163", PlcTagDataType.Int16, "Giới hạn dưới Momen trục Z");
    }

    public static class DataOriginCheck
    {
        public static readonly PlcTagDefinition MoveToOriginCheckSpeedLimit = CreateWord("data.move_to_origin_check_speed_limit", "D21108", PlcTagDataType.Float, "Tốc độ trục X tới vị trí bắt đầu check gốc");
        public static readonly PlcTagDefinition OriginCheckSpeedLimit = CreateWord("data.origin_check_speed_limit", "D21110", PlcTagDataType.Float, "Tốc độ check Gốc");
        public static readonly PlcTagDefinition OffsetX = CreateWord("data.origin_check_offset_x", "D21112", PlcTagDataType.Float, "Ofset tọa độ trục X");
        public static readonly PlcTagDefinition OffsetY = CreateWord("data.origin_check_offset_y", "D21114", PlcTagDataType.Float, "Ofset Tọa độ trục Y");
        public static readonly PlcTagDefinition OffsetZ = CreateWord("data.origin_check_offset_z", "D21116", PlcTagDataType.Float, "Ofset tọa độ trục Z");
        public static readonly PlcTagDefinition JigClampThickness = CreateWord("data.jig_clamp_thickness", "D21118", PlcTagDataType.Float, "Độ dày Jig kẹp");
        public static readonly PlcTagDefinition OriginXLine1 = CreateWord("data.origin_x_line_1", "D21120", PlcTagDataType.Float, "Tọa độ Gốc X Line 1");
        public static readonly PlcTagDefinition OriginYLine1 = CreateWord("data.origin_y_line_1", "D21122", PlcTagDataType.Float, "Tọa độ Gốc Y Line 1");
        public static readonly PlcTagDefinition OriginZLine1 = CreateWord("data.origin_z_line_1", "D21124", PlcTagDataType.Float, "Tọa độ Gốc Z Line 1");
        public static readonly PlcTagDefinition OriginXLine2 = CreateWord("data.origin_x_line_2", "D21126", PlcTagDataType.Float, "Tọa độ Gốc X Line 2");
        public static readonly PlcTagDefinition OriginYLine2 = CreateWord("data.origin_y_line_2", "D21128", PlcTagDataType.Float, "Tọa độ Gốc Y Line 2");
        public static readonly PlcTagDefinition OriginZLine2 = CreateWord("data.origin_z_line_2", "D21130", PlcTagDataType.Float, "Tọa độ Gốc Z Line 2");
    }

    public static class Manual
    {
        public static readonly PlcTagDefinition MoveXForward = CreateBit("manual.move_x_forward", "M2000", "Tiến trục X");
        public static readonly PlcTagDefinition MoveXBackward = CreateBit("manual.move_x_backward", "M2001", "Lùi trục X");
        public static readonly PlcTagDefinition HomeX = CreateBit("manual.home_x", "M2002", "Về góc trục X");
        public static readonly PlcTagDefinition MoveXToPoint = CreateBit("manual.move_x_to_point", "M2003", "Chạy điểm vị trí trục X");
        public static readonly PlcTagDefinition ManualSpeedX = CreateWord("manual.manual_speed_x", "D5150", PlcTagDataType.Float, "Tốc độ Manual trục X (mm/s)");
        public static readonly PlcTagDefinition MovePointX = CreateWord("manual.move_point_x", "D5152", PlcTagDataType.Float, "Vị trí chạy điểm trục X (mm)");
        public static readonly PlcTagDefinition CurrentPositionX = CreateWord("manual.current_position_x", "D5154", PlcTagDataType.Float, "Vị trí hiện tại trục X (hiển thị) (mm)");
        public static readonly PlcTagDefinition IsHomingX = CreateBit("manual.is_homing_x", "M2040", "Đang về Home X");
        public static readonly PlcTagDefinition IsHomedX = CreateBit("manual.is_homed_x", "M2041", "Đã về Home X");

        public static readonly PlcTagDefinition MoveYLeft = CreateBit("manual.move_y_left", "M2004", "Trái trục Y");
        public static readonly PlcTagDefinition MoveYRight = CreateBit("manual.move_y_right", "M2005", "Phải trục Y");
        public static readonly PlcTagDefinition HomeY = CreateBit("manual.home_y", "M2006", "Về góc trục Y");
        public static readonly PlcTagDefinition MoveYToPoint = CreateBit("manual.move_y_to_point", "M2007", "Chạy điểm vị trí trục Y");
        public static readonly PlcTagDefinition ManualSpeedY = CreateWord("manual.manual_speed_y", "D5156", PlcTagDataType.Float, "Tốc độ Manual trục Y (mm/s)");
        public static readonly PlcTagDefinition MovePointY = CreateWord("manual.move_point_y", "D5158", PlcTagDataType.Float, "Vị trí chạy điểm trục Y (mm)");
        public static readonly PlcTagDefinition CurrentPositionY = CreateWord("manual.current_position_y", "D5160", PlcTagDataType.Float, "Vị trí hiện tại trục Y (hiển thị) (mm)");
        public static readonly PlcTagDefinition IsHomingY = CreateBit("manual.is_homing_y", "M2042", "Đang về Home Y");
        public static readonly PlcTagDefinition IsHomedY = CreateBit("manual.is_homed_y", "M2043", "Đã về Home Y");

        public static readonly PlcTagDefinition MoveZUp = CreateBit("manual.move_z_up", "M2008", "Lên trục Z");
        public static readonly PlcTagDefinition MoveZDown = CreateBit("manual.move_z_down", "M2009", "Xuống trục Z");
        public static readonly PlcTagDefinition HomeZ = CreateBit("manual.home_z", "M2010", "Về góc trục Z");
        public static readonly PlcTagDefinition MoveZToPoint = CreateBit("manual.move_z_to_point", "M2011", "Chạy điểm vị trí trục Z");
        public static readonly PlcTagDefinition ManualSpeedZ = CreateWord("manual.manual_speed_z", "D5162", PlcTagDataType.Float, "Tốc độ Manual trục Z (mm/s)");
        public static readonly PlcTagDefinition MovePointZ = CreateWord("manual.move_point_z", "D5164", PlcTagDataType.Float, "Vị trí chạy điểm trục Z (mm)");
        public static readonly PlcTagDefinition CurrentPositionZ = CreateWord("manual.current_position_z", "D5166", PlcTagDataType.Float, "Vị trí hiện tại trục Z (hiển thị) (mm)");
        public static readonly PlcTagDefinition IsHomingZ = CreateBit("manual.is_homing_z", "M2044", "Đang về Home Z");
        public static readonly PlcTagDefinition IsHomedZ = CreateBit("manual.is_homed_z", "M2045", "Đã về Home Z");

        public static readonly PlcTagDefinition ToolClampIn = CreateBit("manual.tool_clamp_in", "M2012", "Xilanh tay tool kẹp vào");
        public static readonly PlcTagDefinition ToolClampOut = CreateBit("manual.tool_clamp_out", "M2013", "Xilanh tay tool mở ra");
        public static readonly PlcTagDefinition ToolRotate0 = CreateBit("manual.tool_rotate_0", "M2014", "Xilanh tay tool điểm 0");
        public static readonly PlcTagDefinition ToolRotate90 = CreateBit("manual.tool_rotate_90", "M2015", "Xilanh tay tool quay 90");
        public static readonly PlcTagDefinition ClampCart1 = CreateBit("manual.clamp_cart_1", "M2016", "Xilanh kẹp xe hàng 1");
        public static readonly PlcTagDefinition UnclampCart1 = CreateBit("manual.unclamp_cart_1", "M2017", "Xilanh mở xe hàng 1");
        public static readonly PlcTagDefinition ClampCart2 = CreateBit("manual.clamp_cart_2", "M2018", "Xilanh kẹp xe hàng 2");
        public static readonly PlcTagDefinition UnclampCart2 = CreateBit("manual.unclamp_cart_2", "M2019", "Xilanh mở xe hàng 2");
        public static readonly PlcTagDefinition HomeAll = CreateBit("manual.home_all", "M2020", "Home ALL");
        public static readonly PlcTagDefinition HomeRotateCylinder = CreateBit("manual.home_rotate_cylinder", "M2023", "Home Xilanh Xoay");
        public static readonly PlcTagDefinition HomeToolClampCylinder = CreateBit("manual.home_tool_clamp_cylinder", "M2024", "Home Xilanh kẹp tay tool");
        public static readonly PlcTagDefinition BuzzerOnOff = CreateBit("manual.buzzer_on_off", "M2030", "Bật tắt còi");
        public static readonly PlcTagDefinition LightCurtainOnOff = CreateBit("manual.light_curtain_on_off", "M2031", "Tắt bật Light Curtain");

        public static readonly PlcTagDefinition Cart1OpenedSignal = CreateBit("manual.cart_1_opened_signal", "M2046", "Tín hiệu đã mở kẹp xe hàng 1");
        public static readonly PlcTagDefinition Cart1ClosedSignal = CreateBit("manual.cart_1_closed_signal", "M2047", "Tín hiệu đã kẹp xe hàng 1");
        public static readonly PlcTagDefinition Cart2OpenedSignal = CreateBit("manual.cart_2_opened_signal", "M2048", "Tín hiệu đã mở kẹp xe hàng 2");
        public static readonly PlcTagDefinition Cart2ClosedSignal = CreateBit("manual.cart_2_closed_signal", "M2049", "Tín hiệu đã kẹp tool");
        public static readonly PlcTagDefinition ToolClosedSignal = CreateBit("manual.tool_closed_signal", "M2050", "Tín hiệu đã kẹp Tool");
        public static readonly PlcTagDefinition ToolOpenedSignal = CreateBit("manual.tool_opened_signal", "M2051", "Tín hiệu đã mở kẹp Tool");
        public static readonly PlcTagDefinition RotatedTo0Signal = CreateBit("manual.rotated_to_0_signal", "M2052", "Tín hiệu đã quay về 0");
        public static readonly PlcTagDefinition RotatedTo90Signal = CreateBit("manual.rotated_to_90_signal", "M2053", "Tín hiệu đã quay về 90");
        public static readonly PlcTagDefinition HomeRotateCylinderDone = CreateBit("manual.home_rotate_cylinder_done", "M2056", "Đã về home xilanh xoay");
        public static readonly PlcTagDefinition HomeToolClampDone = CreateBit("manual.home_tool_clamp_done", "M2057", "Đã về home kẹp tay tool");
        public static readonly PlcTagDefinition IsRunningToPointX = CreateBit("manual.is_running_to_point_x", "M2058", "Đang chạy điểm trục X");
        public static readonly PlcTagDefinition IsRunningToPointY = CreateBit("manual.is_running_to_point_y", "M2059", "Đang chạy điểm trục Y");
        public static readonly PlcTagDefinition IsRunningToPointZ = CreateBit("manual.is_running_to_point_z", "M2060", "Đang chạy điểm trục Z");
    }

      public static class Alarms
    {
        public static readonly PlcTagDefinition AlarmCode1 = CreateWord("alarm.code_1", "D5140", PlcTagDataType.Int16, "Mã cảnh báo 1");
        public static readonly PlcTagDefinition AlarmCode2 = CreateWord("alarm.code_2", "D5141", PlcTagDataType.Int16, "Mã cảnh báo 2");
        public static readonly PlcTagDefinition ErrorCode1 = CreateWord("alarm.error_code_1", "D5142", PlcTagDataType.Int16, "Mã lỗi 1");
        public static readonly PlcTagDefinition ErrorCode2 = CreateWord("alarm.error_code_2", "D5143", PlcTagDataType.Int16, "Mã lỗi 2");
        public static readonly PlcTagDefinition ErrorCode3 = CreateWord("alarm.error_code_3", "D5144", PlcTagDataType.Int16, "Mã lỗi 3");
        public static readonly PlcTagDefinition ErrorCode4 = CreateWord("alarm.error_code_4", "D5145", PlcTagDataType.Int16, "Mã lỗi 4");


        public static readonly PlcTagDefinition HumanInWorkingZone = CreateBit("alarm.human_in_working_zone", "D5140.0", "Cảnh báo có người trong vùng hoạt động");

        public static readonly PlcTagDefinition EStop = CreateBit("alarm.estop", "D5142.0", "Lỗi dừng khẩn cấp");
        public static readonly PlcTagDefinition XLimitNegative = CreateBit("alarm.x_limit_negative", "D5142.1", "Lỗi quá giới hạn mềm phía ngoài trục X");
        public static readonly PlcTagDefinition XLimitPositive = CreateBit("alarm.x_limit_positive", "D5142.2", "Lỗi quá giới hạn mềm phía trong trục X");
        public static readonly PlcTagDefinition YLimitNegative = CreateBit("alarm.y_limit_negative", "D5142.3", "Lỗi quá giới hạn mềm bên trái trục Y");
        public static readonly PlcTagDefinition YLimitPositive = CreateBit("alarm.y_limit_positive", "D5142.4", "Lỗi quá giới hạn mềm bên phải trục Y");
        public static readonly PlcTagDefinition ZLimitNegative = CreateBit("alarm.z_limit_negative", "D5142.5", "Lỗi quá giới hạn mềm phía dưới trục Z");
        public static readonly PlcTagDefinition ZLimitPositive = CreateBit("alarm.z_limit_positive", "D5142.6", "Lỗi quá giới hạn mềm phía trên trục Z");
        public static readonly PlcTagDefinition PickSlip = CreateBit("alarm.pick_slip", "D5142.7", "Lỗi gắp trượt sản phẩm");
        public static readonly PlcTagDefinition PlaceSlip = CreateBit("alarm.place_slip", "D5142.8", "Lỗi thả trượt sản phẩm");
        public static readonly PlcTagDefinition CanOpenDisconnect = CreateBit("alarm.canopen_disconnect", "D5142.9", "Mất kết nối canopen");
        public static readonly PlcTagDefinition LostPhase = CreateBit("alarm.lost_phase", "D5142.10", "Lỗi mất pha");
        public static readonly PlcTagDefinition NotHomed = CreateBit("alarm.not_homed", "D5142.11", "Lỗi chưa về gốc");
        public static readonly PlcTagDefinition AlarmDriverX = CreateBit("alarm.alarm_driver_x", "D5142.12", "Lỗi Driver trục x");
        public static readonly PlcTagDefinition AlarmDriverY = CreateBit("alarm.alarm_driver_y", "D5142.13", "Lỗi Driver trục y");
        public static readonly PlcTagDefinition AlarmDriverZ = CreateBit("alarm.alarm_driver_z", "D5142.14", "Lỗi Driver trục z");

        public static readonly PlcTagDefinition RotateCylinderTimeout0 = CreateBit("alarm.rotate_cylinder_timeout_0", "D5143.0", "Lỗi timeout xilanh xoay 0");
        public static readonly PlcTagDefinition RotateCylinderTimeout90 = CreateBit("alarm.rotate_cylinder_timeout_90", "D5143.1", "Lỗi timeout xilanh xoay 90");
        public static readonly PlcTagDefinition Cart1PositionInvalid = CreateBit("alarm.cart_1_position_invalid", "D5143.2", "Lỗi xe 1 không ở đúng vị trí");
        public static readonly PlcTagDefinition Cart2PositionInvalid = CreateBit("alarm.cart_2_position_invalid", "D5143.3", "Lỗi xe 2 không ở đúng vị trí");
        public static readonly PlcTagDefinition Cart1ClampCylinderFault = CreateBit("alarm.cart_1_clamp_cylinder_fault", "D5143.4", "Lỗi xilanh kẹp xe 1");
        public static readonly PlcTagDefinition Cart2ClampCylinderFault = CreateBit("alarm.cart_2_clamp_cylinder_fault", "D5143.5", "Lỗi xilanh kẹp xe 2");
        public static readonly PlcTagDefinition ToolGripperNoRelease = CreateBit("alarm.tool_gripper_no_release", "D5143.6", "Lỗi tay kẹp không nhả");
        public static readonly PlcTagDefinition ToolGripperNoClamp = CreateBit("alarm.tool_gripper_no_clamp", "D5143.7", "Lỗi tay kẹp không kẹp");
        public static readonly PlcTagDefinition FailPlaceLine1 = CreateBit("alarm.fail_place_line1", "D5143.8", "Lỗi thả sản phẩm line 1");
        public static readonly PlcTagDefinition FailPlaceLine2 = CreateBit("alarm.fail_place_line2", "D5143.9", "Lỗi cắm sản phẩm line 2");

        public static readonly PlcTagDefinition LightCurtain = CreateBit("alarm.light_curtain", "D5144.0", "Lỗi light curtain");
        public static readonly PlcTagDefinition XSoftLimitOutside = CreateBit("alarm.x_soft_limit_outside", "D5144.1", "Lỗi quá giới hạn phía ngoài trục X");
        public static readonly PlcTagDefinition XSoftLimitInside = CreateBit("alarm.x_soft_limit_inside", "D5144.2", "Lỗi quá giới hạn phía trong trục X");
        public static readonly PlcTagDefinition YSoftLimitLeft = CreateBit("alarm.y_soft_limit_left", "D5144.3", "Lỗi quá giới hạn bên trái trục Y");
        public static readonly PlcTagDefinition YSoftLimitRight = CreateBit("alarm.y_soft_limit_right", "D5144.4", "Lỗi quá giới hạn bên phải trục Y");
        public static readonly PlcTagDefinition ZSoftLimitTop = CreateBit("alarm.z_soft_limit_top", "D5144.5", "Lỗi quá giới hạn phía trên trục Z");
        public static readonly PlcTagDefinition ZSoftLimitBottom = CreateBit("alarm.z_soft_limit_bottom", "D5144.6", "Lỗi quá giới hạn phía dưới trục Z");
        public static readonly PlcTagDefinition AirPressureLost = CreateBit("alarm.air_pressure_lost", "D5144.8", "Lỗi mất khí");
        public static readonly PlcTagDefinition XOverMoment = CreateBit("alarm.x_over_moment", "D5144.9", "Lỗi quá momen trục X");
        public static readonly PlcTagDefinition YOverMoment = CreateBit("alarm.y_over_moment", "D5144.10", "Lỗi quá momen trục Y");
        public static readonly PlcTagDefinition ZOverMoment = CreateBit("alarm.z_over_moment", "D5144.11", "Lỗi quá momen trục Z");

        public static readonly PlcTagDefinition PlcLine1Disconnected = CreateBit("alarm.plc_line_1_disconnected", "D5145.0", "Mất kết nối PLC line 1");
        public static readonly PlcTagDefinition PlcLine2Disconnected = CreateBit("alarm.plc_line_2_disconnected", "D5145.1", "Mất kết nối PLC line 2");
        public static readonly PlcTagDefinition PcDisconnected = CreateBit("alarm.pc_disconnected", "D5145.2", "Mất kết nối PLC line 2");
        public static readonly PlcTagDefinition OrderNotEntered = CreateBit("alarm.order_not_entered", "D5145.3", "Chưa nhập order");
        public static readonly PlcTagDefinition ProductParametersMissingLine1 = CreateBit("alarm.product_parameters_missing_line_1", "D5145.4", "Lỗi thiếu thông số sản phẩm line 1");
        public static readonly PlcTagDefinition ProductParametersMissingLine2 = CreateBit("alarm.product_parameters_missing_line_2", "D5145.5", "Lỗi thiếu thông số sản phẩm line 2");
    }

    public static class ConfirmMessages
    {
        public static readonly PlcTagDefinition CollisionRotate90 = CreateBit("message.collision_rotate_90", "D5650.0", "Đang ở trong vùng va chạm không thể quay 90 xilanh");
        public static readonly PlcTagDefinition CollisionRotate0 = CreateBit("message.collision_rotate_0", "D5650.1", "Đang ở trong vùng va chạm không thể quay 0 xilanh");
        public static readonly PlcTagDefinition NotRotated90 = CreateBit("message.not_rotated_90", "D5650.2", "Xilanh Xoay chưa quay 90 độ");
        public static readonly PlcTagDefinition ZNotHomed = CreateBit("message.z_not_homed", "D5650.3", "Trục Z chưa về gốc");
        public static readonly PlcTagDefinition OpenGripperBeforeHome = CreateBit("message.open_gripper_before_home", "D5650.4", "Mở tay kẹp trước khi về gốc");
        public static readonly PlcTagDefinition ProductionCompleted = CreateBit("message.production_completed", "D5650.5", "Đã Hoàn Thành Sản Xuất");
    }

    /// <summary>
    /// Tags cho PLC Line 1 (192.168.1.6:502) — nhóm Data Auto + Edit Model.
    /// Được đọc/ghi qua kết nối PLC Line 1.
    /// </summary>
    public static class Line1
    {
        // --- Data Auto ---
        public static readonly PlcTagDefinition AutoModelName = CreateWord("line1.auto.model_name", "D6000", PlcTagDataType.String, "Tên Model", 40);
        public static readonly PlcTagDefinition AutoJigType = CreateWord("line1.auto.jig_type", "D6020", PlcTagDataType.Int32, "Loại tay kẹp Line 1");
        public static readonly PlcTagDefinition AutoPickInputX = CreateWord("line1.auto.pick_input_x", "D6022", PlcTagDataType.Float, "Tọa độ X gắp sản phẩm đầu vào line");
        public static readonly PlcTagDefinition AutoPickInputZ = CreateWord("line1.auto.pick_input_z", "D6024", PlcTagDataType.Float, "Tọa độ Z gắp sản phẩm đầu vào line");
        public static readonly PlcTagDefinition AutoPickOp1X = CreateWord("line1.auto.pick_op1_x", "D6026", PlcTagDataType.Float, "Tọa độ X an toàn lên xuống Op1");
        public static readonly PlcTagDefinition AutoPickOp1Z = CreateWord("line1.auto.pick_op1_z", "D6028", PlcTagDataType.Float, "Tọa độ Z an toàn lên xuống Op1");
        public static readonly PlcTagDefinition AutoPickOp2X = CreateWord("line1.auto.pick_op2_x", "D6030", PlcTagDataType.Float, "Tọa độ X an toàn lên xuống Op2");
        public static readonly PlcTagDefinition AutoPickOp2Z = CreateWord("line1.auto.pick_op2_z", "D6032", PlcTagDataType.Float, "Tọa độ Z an toàn lên xuống Op2");
        public static readonly PlcTagDefinition AutoDropOp1X = CreateWord("line1.auto.drop_op1_x", "D6034", PlcTagDataType.Float, "Tọa độ X chống tâm Op1");
        public static readonly PlcTagDefinition AutoDropOp1Z = CreateWord("line1.auto.drop_op1_z", "D6036", PlcTagDataType.Float, "Tọa độ Z chống tâm Op1");
        public static readonly PlcTagDefinition AutoDropOp2X = CreateWord("line1.auto.drop_op2_x", "D6038", PlcTagDataType.Float, "Tọa độ X chống tâm Op2");
        public static readonly PlcTagDefinition AutoDropOp2Z = CreateWord("line1.auto.drop_op2_z", "D6040", PlcTagDataType.Float, "Tọa độ Z chống tâm Op2");
        public static readonly PlcTagDefinition AutoDropMeasureX = CreateWord("line1.auto.drop_measure_x", "D6042", PlcTagDataType.Float, "Tọa độ X chống tâm máy đo");
        public static readonly PlcTagDefinition AutoDropMeasureZ = CreateWord("line1.auto.drop_measure_z", "D6044", PlcTagDataType.Float, "Tọa độ Z chống tâm máy đo");
        public static readonly PlcTagDefinition AutoJigSupportInput = CreateWord("line1.auto.jig_support_input", "D6046", PlcTagDataType.Float, "Tọa độ Jig đỡ trục đầu vào");
        public static readonly PlcTagDefinition AutoGrindTimeOp1 = CreateWord("line1.auto.grind_time_op1", "D6048", PlcTagDataType.Int16, "Thời gian mài Op1");
        public static readonly PlcTagDefinition AutoGrindTimeOp2 = CreateWord("line1.auto.grind_time_op2", "D6049", PlcTagDataType.Int16, "Thời gian mài Op2");
        public static readonly PlcTagDefinition AutoProgramId = CreateWord("line1.auto.program_id", "D6050", PlcTagDataType.Int16, "ProgramID");
        public static readonly PlcTagDefinition AutoLoadDataModel = CreateBit("line1.auto.load_data_model", "D6055.0", "Load dataModel");
        public static readonly PlcTagDefinition AutoDoneLoadDataModel = CreateBit("line1.auto.done_load_data_model", "D6056.0", "Done LoaddataModel");
        public static readonly PlcTagDefinition AutoModelNameToLoad = CreateWord("line1.auto.model_name_to_load", "D6060", PlcTagDataType.String, "Tên Model cần Load", 30);
        public static readonly PlcTagDefinition AutoDiameterOp1 = CreateWord("line1.auto.diameter_op1", "D6076", PlcTagDataType.Float, "Đường kính Op1");
        public static readonly PlcTagDefinition AutoDiameterOp2 = CreateWord("line1.auto.diameter_op2", "D6078", PlcTagDataType.Float, "Đường kính Op2");

        // --- Edit Model Data ---
        public static readonly PlcTagDefinition IdModel = CreateWord("line1.edit.id_model", "D6080", PlcTagDataType.String, "ID Model", 40);
        public static readonly PlcTagDefinition JigType = CreateWord("line1.edit.jig_type", "D6100", PlcTagDataType.Int32, "Loại tay kẹp Line 1");
        public static readonly PlcTagDefinition PickInputX = CreateWord("line1.edit.pick_input_x", "D6102", PlcTagDataType.Float, "Tọa độ X gắp sản phẩm đầu vào line");
        public static readonly PlcTagDefinition PickInputZ = CreateWord("line1.edit.pick_input_z", "D6104", PlcTagDataType.Float, "Tọa độ Z gắp sản phẩm đầu vào line");
        public static readonly PlcTagDefinition PickOp1X = CreateWord("line1.edit.pick_op1_x", "D6106", PlcTagDataType.Float, "Tọa độ X an toàn lên xuống Op1");
        public static readonly PlcTagDefinition PickOp1Z = CreateWord("line1.edit.pick_op1_z", "D6108", PlcTagDataType.Float, "Tọa độ Z an toàn lên xuống Op1");
        public static readonly PlcTagDefinition PickOp2X = CreateWord("line1.edit.pick_op2_x", "D6110", PlcTagDataType.Float, "Tọa độ X an toàn lên xuống Op2");
        public static readonly PlcTagDefinition PickOp2Z = CreateWord("line1.edit.pick_op2_z", "D6112", PlcTagDataType.Float, "Tọa độ Z an toàn lên xuống Op2");
        public static readonly PlcTagDefinition DropOp1X = CreateWord("line1.edit.drop_op1_x", "D6114", PlcTagDataType.Float, "Tọa độ X chống tâm Op1");
        public static readonly PlcTagDefinition DropOp1Z = CreateWord("line1.edit.drop_op1_z", "D6116", PlcTagDataType.Float, "Tọa độ Z chống tâm Op1");
        public static readonly PlcTagDefinition DropOp2X = CreateWord("line1.edit.drop_op2_x", "D6118", PlcTagDataType.Float, "Tọa độ X chống tâm Op2");
        public static readonly PlcTagDefinition DropOp2Z = CreateWord("line1.edit.drop_op2_z", "D6120", PlcTagDataType.Float, "Tọa độ Z chống tâm Op2");
        public static readonly PlcTagDefinition DropMeasureX = CreateWord("line1.edit.drop_measure_x", "D6122", PlcTagDataType.Float, "Tọa độ X chống tâm máy đo");
        public static readonly PlcTagDefinition DropMeasureZ = CreateWord("line1.edit.drop_measure_z", "D6124", PlcTagDataType.Float, "Tọa độ Z chống tâm máy đo");
        public static readonly PlcTagDefinition JigSupportInput = CreateWord("line1.edit.jig_support_input", "D6126", PlcTagDataType.Float, "Tọa độ Jig đỡ trục đầu vào");
        public static readonly PlcTagDefinition GrindTimeOp1 = CreateWord("line1.edit.grind_time_op1", "D6128", PlcTagDataType.Int32, "Thời gian mài Op1");
        public static readonly PlcTagDefinition GrindTimeOp2 = CreateWord("line1.edit.grind_time_op2", "D6130", PlcTagDataType.Int32, "Thời gian mài Op2");
        public static readonly PlcTagDefinition DiameterOp1 = CreateWord("line1.edit.diameter_op1", "D6132", PlcTagDataType.Float, "Đường kính Op1");
        public static readonly PlcTagDefinition DiameterOp2 = CreateWord("line1.edit.diameter_op2", "D6134", PlcTagDataType.Float, "Đường kính Op2");

        // --- Edit Model Search ---
        public static readonly PlcTagDefinition ModelSearchName = CreateWord("line1.edit.model_search_name", "D6210", PlcTagDataType.String, "Tên Model tìm kiếm", 30);
        public static readonly PlcTagDefinition ModelResult1 = CreateWord("line1.edit.model_result_1", "D6225", PlcTagDataType.String, "Tên Model trả về 1", 30);
        public static readonly PlcTagDefinition ModelResult2 = CreateWord("line1.edit.model_result_2", "D6240", PlcTagDataType.String, "Tên Model trả về 2", 30);
        public static readonly PlcTagDefinition ModelResult3 = CreateWord("line1.edit.model_result_3", "D6255", PlcTagDataType.String, "Tên Model trả về 3", 30);
        public static readonly PlcTagDefinition ModelResult4 = CreateWord("line1.edit.model_result_4", "D6270", PlcTagDataType.String, "Tên Model trả về 4", 30);
        public static readonly PlcTagDefinition ModelResult5 = CreateWord("line1.edit.model_result_5", "D6285", PlcTagDataType.String, "Tên Model trả về 5", 30);
        public static readonly PlcTagDefinition ModelResult6 = CreateWord("line1.edit.model_result_6", "D6300", PlcTagDataType.String, "Tên Model trả về 6", 30);

        // --- Edit Control Bits ---
        public static readonly PlcTagDefinition Search = CreateBit("line1.edit.search", "D6160.0", "Tìm kiếm");
        public static readonly PlcTagDefinition EditModel = CreateBit("line1.edit.edit_model", "D6160.1", "Edit");
        public static readonly PlcTagDefinition Next = CreateBit("line1.edit.next", "D6160.2", "Next");
        public static readonly PlcTagDefinition Previous = CreateBit("line1.edit.previous", "D6160.3", "Previous");
        public static readonly PlcTagDefinition SaveModel = CreateBit("line1.edit.save_model", "D6160.4", "Lưu Model");
        public static readonly PlcTagDefinition SaveSuccess = CreateBit("line1.edit.save_success", "D6161.0", "Lưu thành công");
        public static readonly PlcTagDefinition ErrorFlag = CreateBit("line1.edit.error_flag", "D6161.1", "Cờ lỗi");

        // --- Pagination ---
        public static readonly PlcTagDefinition Page = CreateWord("line1.edit.page", "D6170", PlcTagDataType.Int16, "Trang");
        public static readonly PlcTagDefinition TotalPages = CreateWord("line1.edit.total_pages", "D6172", PlcTagDataType.Int16, "Tổng số trang");

        // --- Authentication ---
        public static readonly PlcTagDefinition AccountName = CreateWord("line1.edit.account_name", "D6180", PlcTagDataType.String, "Tên tài khoản", 30);
        public static readonly PlcTagDefinition Password = CreateWord("line1.edit.password", "D6195", PlcTagDataType.String, "Mật khẩu", 30);

        // --- Error Message ---
        public static readonly PlcTagDefinition ErrorMessage = CreateWord("line1.edit.error_message", "D6315", PlcTagDataType.String, "Trả về message lỗi", 40);
    }

    /// <summary>
    /// Tags cho PLC Line 2 (192.168.1.7:502) — nhóm Data Auto + Edit Model.
    /// Được đọc/ghi qua kết nối PLC Line 2. Cùng địa chỉ với Line 1.
    /// </summary>
    public static class Line2
    {
        // --- Data Auto ---
        public static readonly PlcTagDefinition AutoModelName = CreateWord("line2.auto.model_name", "D6000", PlcTagDataType.String, "Tên Model", 40);
        public static readonly PlcTagDefinition AutoJigType = CreateWord("line2.auto.jig_type", "D6020", PlcTagDataType.Int32, "Loại tay kẹp Line 2");
        public static readonly PlcTagDefinition AutoPickInputX = CreateWord("line2.auto.pick_input_x", "D6022", PlcTagDataType.Float, "Tọa độ X gắp sản phẩm đầu vào line");
        public static readonly PlcTagDefinition AutoPickInputZ = CreateWord("line2.auto.pick_input_z", "D6024", PlcTagDataType.Float, "Tọa độ Z gắp sản phẩm đầu vào line");
        public static readonly PlcTagDefinition AutoPickOp1X = CreateWord("line2.auto.pick_op1_x", "D6026", PlcTagDataType.Float, "Tọa độ X an toàn lên xuống Op1");
        public static readonly PlcTagDefinition AutoPickOp1Z = CreateWord("line2.auto.pick_op1_z", "D6028", PlcTagDataType.Float, "Tọa độ Z an toàn lên xuống Op1");
        public static readonly PlcTagDefinition AutoPickOp2X = CreateWord("line2.auto.pick_op2_x", "D6030", PlcTagDataType.Float, "Tọa độ X an toàn lên xuống Op2");
        public static readonly PlcTagDefinition AutoPickOp2Z = CreateWord("line2.auto.pick_op2_z", "D6032", PlcTagDataType.Float, "Tọa độ Z an toàn lên xuống Op2");
        public static readonly PlcTagDefinition AutoDropOp1X = CreateWord("line2.auto.drop_op1_x", "D6034", PlcTagDataType.Float, "Tọa độ X chống tâm Op1");
        public static readonly PlcTagDefinition AutoDropOp1Z = CreateWord("line2.auto.drop_op1_z", "D6036", PlcTagDataType.Float, "Tọa độ Z chống tâm Op1");
        public static readonly PlcTagDefinition AutoDropOp2X = CreateWord("line2.auto.drop_op2_x", "D6038", PlcTagDataType.Float, "Tọa độ X chống tâm Op2");
        public static readonly PlcTagDefinition AutoDropOp2Z = CreateWord("line2.auto.drop_op2_z", "D6040", PlcTagDataType.Float, "Tọa độ Z chống tâm Op2");
        public static readonly PlcTagDefinition AutoDropMeasureX = CreateWord("line2.auto.drop_measure_x", "D6042", PlcTagDataType.Float, "Tọa độ X chống tâm máy đo");
        public static readonly PlcTagDefinition AutoDropMeasureZ = CreateWord("line2.auto.drop_measure_z", "D6044", PlcTagDataType.Float, "Tọa độ Z chống tâm máy đo");
        public static readonly PlcTagDefinition AutoJigSupportInput = CreateWord("line2.auto.jig_support_input", "D6046", PlcTagDataType.Float, "Tọa độ Jig đỡ trục đầu vào");
        public static readonly PlcTagDefinition AutoGrindTimeOp1 = CreateWord("line2.auto.grind_time_op1", "D6048", PlcTagDataType.Int16, "Thời gian mài Op1");
        public static readonly PlcTagDefinition AutoGrindTimeOp2 = CreateWord("line2.auto.grind_time_op2", "D6049", PlcTagDataType.Int16, "Thời gian mài Op2");
        public static readonly PlcTagDefinition AutoProgramId = CreateWord("line2.auto.program_id", "D6050", PlcTagDataType.Int16, "ProgramID");
        public static readonly PlcTagDefinition AutoLoadDataModel = CreateBit("line2.auto.load_data_model", "D6055.0", "Load dataModel");
        public static readonly PlcTagDefinition AutoDoneLoadDataModel = CreateBit("line2.auto.done_load_data_model", "D6056.0", "Done LoaddataModel");
        public static readonly PlcTagDefinition AutoModelNameToLoad = CreateWord("line2.auto.model_name_to_load", "D6060", PlcTagDataType.String, "Tên Model cần Load", 30);
        public static readonly PlcTagDefinition AutoDiameterOp1 = CreateWord("line2.auto.diameter_op1", "D6076", PlcTagDataType.Float, "Đường kính Op1");
        public static readonly PlcTagDefinition AutoDiameterOp2 = CreateWord("line2.auto.diameter_op2", "D6078", PlcTagDataType.Float, "Đường kính Op2");

        // --- Edit Model Data ---
        public static readonly PlcTagDefinition IdModel = CreateWord("line2.edit.id_model", "D6080", PlcTagDataType.String, "ID Model", 40);
        public static readonly PlcTagDefinition JigType = CreateWord("line2.edit.jig_type", "D6100", PlcTagDataType.Int32, "Loại tay kẹp Line 2");
        public static readonly PlcTagDefinition PickInputX = CreateWord("line2.edit.pick_input_x", "D6102", PlcTagDataType.Float, "Tọa độ X gắp sản phẩm đầu vào line");
        public static readonly PlcTagDefinition PickInputZ = CreateWord("line2.edit.pick_input_z", "D6104", PlcTagDataType.Float, "Tọa độ Z gắp sản phẩm đầu vào line");
        public static readonly PlcTagDefinition PickOp1X = CreateWord("line2.edit.pick_op1_x", "D6106", PlcTagDataType.Float, "Tọa độ X an toàn lên xuống Op1");
        public static readonly PlcTagDefinition PickOp1Z = CreateWord("line2.edit.pick_op1_z", "D6108", PlcTagDataType.Float, "Tọa độ Z an toàn lên xuống Op1");
        public static readonly PlcTagDefinition PickOp2X = CreateWord("line2.edit.pick_op2_x", "D6110", PlcTagDataType.Float, "Tọa độ X an toàn lên xuống Op2");
        public static readonly PlcTagDefinition PickOp2Z = CreateWord("line2.edit.pick_op2_z", "D6112", PlcTagDataType.Float, "Tọa độ Z an toàn lên xuống Op2");
        public static readonly PlcTagDefinition DropOp1X = CreateWord("line2.edit.drop_op1_x", "D6114", PlcTagDataType.Float, "Tọa độ X chống tâm Op1");
        public static readonly PlcTagDefinition DropOp1Z = CreateWord("line2.edit.drop_op1_z", "D6116", PlcTagDataType.Float, "Tọa độ Z chống tâm Op1");
        public static readonly PlcTagDefinition DropOp2X = CreateWord("line2.edit.drop_op2_x", "D6118", PlcTagDataType.Float, "Tọa độ X chống tâm Op2");
        public static readonly PlcTagDefinition DropOp2Z = CreateWord("line2.edit.drop_op2_z", "D6120", PlcTagDataType.Float, "Tọa độ Z chống tâm Op2");
        public static readonly PlcTagDefinition DropMeasureX = CreateWord("line2.edit.drop_measure_x", "D6122", PlcTagDataType.Float, "Tọa độ X chống tâm máy đo");
        public static readonly PlcTagDefinition DropMeasureZ = CreateWord("line2.edit.drop_measure_z", "D6124", PlcTagDataType.Float, "Tọa độ Z chống tâm máy đo");
        public static readonly PlcTagDefinition JigSupportInput = CreateWord("line2.edit.jig_support_input", "D6126", PlcTagDataType.Float, "Tọa độ Jig đỡ trục đầu vào");
        public static readonly PlcTagDefinition GrindTimeOp1 = CreateWord("line2.edit.grind_time_op1", "D6128", PlcTagDataType.Int32, "Thời gian mài Op1");
        public static readonly PlcTagDefinition GrindTimeOp2 = CreateWord("line2.edit.grind_time_op2", "D6130", PlcTagDataType.Int32, "Thời gian mài Op2");
        public static readonly PlcTagDefinition DiameterOp1 = CreateWord("line2.edit.diameter_op1", "D6132", PlcTagDataType.Float, "Đường kính Op1");
        public static readonly PlcTagDefinition DiameterOp2 = CreateWord("line2.edit.diameter_op2", "D6134", PlcTagDataType.Float, "Đường kính Op2");

        // --- Edit Model Search ---
        public static readonly PlcTagDefinition ModelSearchName = CreateWord("line2.edit.model_search_name", "D6210", PlcTagDataType.String, "Tên Model tìm kiếm", 30);
        public static readonly PlcTagDefinition ModelResult1 = CreateWord("line2.edit.model_result_1", "D6225", PlcTagDataType.String, "Tên Model trả về 1", 30);
        public static readonly PlcTagDefinition ModelResult2 = CreateWord("line2.edit.model_result_2", "D6240", PlcTagDataType.String, "Tên Model trả về 2", 30);
        public static readonly PlcTagDefinition ModelResult3 = CreateWord("line2.edit.model_result_3", "D6255", PlcTagDataType.String, "Tên Model trả về 3", 30);
        public static readonly PlcTagDefinition ModelResult4 = CreateWord("line2.edit.model_result_4", "D6270", PlcTagDataType.String, "Tên Model trả về 4", 30);
        public static readonly PlcTagDefinition ModelResult5 = CreateWord("line2.edit.model_result_5", "D6285", PlcTagDataType.String, "Tên Model trả về 5", 30);
        public static readonly PlcTagDefinition ModelResult6 = CreateWord("line2.edit.model_result_6", "D6300", PlcTagDataType.String, "Tên Model trả về 6", 30);

        // --- Edit Control Bits ---
        public static readonly PlcTagDefinition Search = CreateBit("line2.edit.search", "D6160.0", "Tìm kiếm");
        public static readonly PlcTagDefinition EditModel = CreateBit("line2.edit.edit_model", "D6160.1", "Edit");
        public static readonly PlcTagDefinition Next = CreateBit("line2.edit.next", "D6160.2", "Next");
        public static readonly PlcTagDefinition Previous = CreateBit("line2.edit.previous", "D6160.3", "Previous");
        public static readonly PlcTagDefinition SaveModel = CreateBit("line2.edit.save_model", "D6160.4", "Lưu Model");
        public static readonly PlcTagDefinition SaveSuccess = CreateBit("line2.edit.save_success", "D6161.0", "Lưu thành công");
        public static readonly PlcTagDefinition ErrorFlag = CreateBit("line2.edit.error_flag", "D6161.1", "Cờ lỗi");

        // --- Pagination ---
        public static readonly PlcTagDefinition Page = CreateWord("line2.edit.page", "D6170", PlcTagDataType.Int16, "Trang");
        public static readonly PlcTagDefinition TotalPages = CreateWord("line2.edit.total_pages", "D6172", PlcTagDataType.Int16, "Tổng số trang");

        // --- Authentication ---
        public static readonly PlcTagDefinition AccountName = CreateWord("line2.edit.account_name", "D6180", PlcTagDataType.String, "Tên tài khoản", 30);
        public static readonly PlcTagDefinition Password = CreateWord("line2.edit.password", "D6195", PlcTagDataType.String, "Mật khẩu", 30);

        // --- Error Message ---
        public static readonly PlcTagDefinition ErrorMessage = CreateWord("line2.edit.error_message", "D6315", PlcTagDataType.String, "Trả về message lỗi", 40);
    }

    public static class Agv
    {
        // === Inputs từ PLC (đọc) ===
        public static readonly PlcTagDefinition MachineReadyForSwapLine1 =
            CreateBit("agv.machine_ready_swap_line1", "D5642.0", "Máy báo sẵn sàng cho đảo kệ line 1");

        public static readonly PlcTagDefinition MachineReadyForSwapLine2 =
            CreateBit("agv.machine_ready_swap_line2", "D5642.1", "Máy báo sẵn sàng cho đảo kệ line 2");

        // === Outputs xuống PLC (ghi) ===
        public static readonly PlcTagDefinition AgvRequestSwapLine1 =
            CreateBit("agv.request_swap_line1", "D5641.0", "AGV yêu cầu đảo kệ line 1");

        public static readonly PlcTagDefinition AgvRequestSwapLine2 =
            CreateBit("agv.request_swap_line2", "D5641.1", "AGV yêu cầu đảo kệ line 2");

        public static readonly PlcTagDefinition AgvSwapDoneLine1 =
            CreateBit("agv.swap_done_line1", "D5641.2", "AGV báo đã đảo xong kệ line 1");

        public static readonly PlcTagDefinition AgvSwapDoneLine2 =
            CreateBit("agv.swap_done_line2", "D5641.3", "AGV báo đã đảo xong kệ line 2");
    }

    public static IReadOnlyList<PlcTagDefinition> All { get; } =
    [
        Words.D5120,
        Words.D5121,
        Words.D5122,
        Words.D5123,
        Words.D5124,
        Words.D5130,
        Words.D5131,
        Inputs.X0_00,
        Inputs.X0_01,
        Inputs.X0_02,
        Inputs.X0_03,
        Inputs.X0_04,
        Inputs.X0_05,
        Inputs.X0_06,
        Inputs.X0_07,
        Inputs.X0_08,
        Inputs.X0_09,
        Inputs.X0_10,
        Inputs.X0_11,
        Inputs.X0_12,
        Inputs.X0_13,
        Inputs.X0_14,
        Inputs.X0_15,
        Inputs.X1_00,
        Inputs.X1_01,
        Inputs.X1_02,
        Inputs.X1_03,
        Inputs.X1_04,
        Inputs.X1_05,
        Inputs.X1_06,
        Inputs.X1_07,
        Inputs.X1_08,
        Inputs.X1_09,
        Inputs.X1_10,
        Inputs.X1_11,
        Inputs.X1_12,
        Inputs.X1_13,
        Inputs.X1_14,
        Inputs.X1_15,
        Inputs.X2_00,
        Inputs.X2_01,
        Inputs.X2_02,
        Inputs.X2_03,
        Inputs.X2_04,
        Inputs.X2_05,
        Inputs.X2_06,
        Inputs.X2_07,
        Inputs.X2_08,
        Inputs.X2_09,
        Inputs.X2_10,
        Inputs.X2_11,
        Inputs.X2_12,
        Inputs.X2_13,
        Inputs.X2_14,
        Inputs.X2_15,
        Inputs.X3_00,
        Inputs.X3_01,
        Inputs.X3_02,
        Inputs.X3_03,
        Inputs.X3_04,
        Inputs.X3_05,
        Inputs.X3_06,
        Inputs.X3_07,
        Inputs.X3_08,
        Inputs.X3_09,
        Inputs.X3_10,
        Inputs.X3_11,
        Inputs.X3_12,
        Inputs.X3_13,
        Inputs.X3_14,
        Inputs.X3_15,
        Inputs.X4_00,
        Inputs.X4_01,
        Inputs.X4_02,
        Inputs.X4_03,
        Inputs.X4_04,
        Inputs.X4_05,
        Inputs.X4_06,
        Inputs.X4_07,
        Inputs.X4_08,
        Inputs.X4_09,
        Inputs.X4_10,
        Inputs.X4_11,
        Inputs.X4_12,
        Inputs.X4_13,
        Inputs.X4_14,
        Inputs.X4_15,
        Outputs.Y0_00,
        Outputs.Y0_01,
        Outputs.Y0_02,
        Outputs.Y0_03,
        Outputs.Y0_04,
        Outputs.Y0_05,
        Outputs.Y0_06,
        Outputs.Y0_07,
        Outputs.Y0_08,
        Outputs.Y0_09AxisXOn,
        Outputs.Y0_10AxisYOn,
        Outputs.Y0_11AxisZOn,
        Outputs.Y0_12,
        Outputs.Y0_13,
        Outputs.Y0_14,
        Outputs.Y0_15,
        Outputs.Y1_00LightRed,
        Outputs.Y1_01LightYellow,
        Outputs.Y1_02LightGreen,
        Outputs.Y1_03LightBuzzer,
        Outputs.Y1_04XlCenter1In,
        Outputs.Y1_05XlCenter1Out,
        Outputs.Y1_06XlCenter2In,
        Outputs.Y1_07XlCenter2Out,
        Outputs.Y1_08XlClampPartIn,
        Outputs.Y1_09XlClampPartOut,
        Outputs.Y1_10XlRotary0,
        Outputs.Y1_11XlRotary90,
        Outputs.Y1_12BtLightStop,
        Outputs.Y1_13BtLightStart,
        Outputs.Y1_14BtLightReset,
        Outputs.Y1_15,
        DataAutos.Shelf1ProductCount,
        DataAutos.Shelf1OrderCount,
        DataAutos.OrderLine1Code,
        DataAutos.OrderLine1ModelId,
        DataAutos.OrderLine1Quantity,
        DataAutos.OrderLine1JigType,
        DataAutos.OrderLine1StartPosition,
        DataAutos.OrderLine1TrayIndex,
        DataAutos.OrderLine1TrayType,
        DataAutos.OrderLine1Sequence,
        //DataAutos.OrderLine1CheckPoint1X,
        //DataAutos.OrderLine1CheckPoint1Y,
        //DataAutos.OrderLine1CheckPoint1Z,
        DataAutos.OrderLine1PartHoverHeight,
        DataAutos.OrderLine1JigCenterOffset,
        DataAutos.OrderLine1JigDepthOffset,
        DataAutos.OrderLine1PickedCount,
        DataAutos.OrderLine1DiameterOp1,
        DataAutos.OrderLine1IsLoading,
        DataAutos.OrderLine1ProductionResultAcknowledged,
        DataAutos.OrderLine1PausedByPc,
        DataAutos.OrderLine1ClearRequestedByPc,
        DataAutos.OrderLine1ShelfOrdersCompleted,
        DataAutos.CurrentOrderRunStarted,
        DataAutos.CurrentOrderLoadCompleted,
        DataAutos.CurrentOrderCompleted,
        DataAutos.OrderLine1CurrentPickIndex,
        DataAutos.Shelf2ProductCount,
        DataAutos.Shelf2OrderCount,
        DataAutos.OrderLine2Code,
        DataAutos.OrderLine2ModelId,
        DataAutos.OrderLine2Quantity,
        DataAutos.OrderLine2JigType,
        DataAutos.OrderLine2StartPosition,
        DataAutos.OrderLine2TrayIndex,
        DataAutos.OrderLine2TrayType,
        DataAutos.OrderLine2Sequence,
        //DataAutos.OrderLine2CheckPoint1X,
        //DataAutos.OrderLine2CheckPoint1Y,
        //DataAutos.OrderLine2CheckPoint1Z,
        DataAutos.OrderLine2PartHoverHeight,
        DataAutos.OrderLine2JigCenterOffset,
        DataAutos.OrderLine2JigDepthOffset,
        DataAutos.OrderLine2PickedCount,
        DataAutos.OrderLine2DiameterOp1,
        DataAutos.OrderLine2IsLoading,
        DataAutos.OrderLine2ProductionResultAcknowledged,
        DataAutos.OrderLine2PausedByPc,
        DataAutos.OrderLine2ClearRequestedByPc,
        DataAutos.OrderLine2ShelfOrdersCompleted,
        DataAutos.CurrentOrderRunStartedLine2,
        DataAutos.CurrentOrderLoadCompletedLine2,
        DataAutos.CurrentOrderCompletedLine2,
        DataAutos.OrderLine2CurrentPickIndex,
        DataAutos.Clock1s,
        RobotTest.JigProductHeight,
        RobotTest.JigCenterOffset,
        RobotTest.JigDepthOffset,
        RobotTest.DiameterOp1,
        RobotTest.TrayType,
        RobotTest.RunLine1,
        RobotTest.RunLine2,
        RobotTest.CancelPickLine1,
        RobotTest.CancelPickLine2,
        DataTrayCart.Cart1SmallTrayPos1X,
        DataTrayCart.Cart1SmallTrayPos1Y,
        DataTrayCart.Cart1SmallTrayPos1Z,
        DataTrayCart.Cart1SmallTrayPos2X,
        DataTrayCart.Cart1SmallTrayPos2Y,
        DataTrayCart.Cart1SmallTrayPos2Z,
        DataTrayCart.Cart1LargeTrayPos1X,
        DataTrayCart.Cart1LargeTrayPos1Y,
        DataTrayCart.Cart1LargeTrayPos1Z,
        DataTrayCart.Cart1LargeTrayPos2X,
        DataTrayCart.Cart1LargeTrayPos2Y,
        DataTrayCart.Cart1LargeTrayPos2Z,
        DataTrayCart.Cart2SmallTrayPos1X,
        DataTrayCart.Cart2SmallTrayPos1Y,
        DataTrayCart.Cart2SmallTrayPos1Z,
        DataTrayCart.Cart2SmallTrayPos2X,
        DataTrayCart.Cart2SmallTrayPos2Y,
        DataTrayCart.Cart2SmallTrayPos2Z,
        DataTrayCart.Cart2LargeTrayPos1X,
        DataTrayCart.Cart2LargeTrayPos1Y,
        DataTrayCart.Cart2LargeTrayPos1Z,
        DataTrayCart.Cart2LargeTrayPos2X,
        DataTrayCart.Cart2LargeTrayPos2Y,
        DataTrayCart.Cart2LargeTrayPos2Z,
        DataTrayCart.SmallTrayColumnSpacing,
        DataTrayCart.SmallTrayRowSpacing,
        DataTrayCart.SmallTrayJigHeight,
        DataTrayCart.SmallTrayColumns,
        DataTrayCart.SmallTrayRows,
        DataTrayCart.LargeTrayColumnSpacing,
        DataTrayCart.LargeTrayRowSpacing,
        DataTrayCart.LargeTrayColumns,
        DataTrayCart.LargeTrayRows,
        DataTrayCart.LargeTrayJigHeight,
        DataMachine.AxisXSpeedLimit,
        DataMachine.AxisYSpeedLimit,
        DataMachine.AxisZSpeedLimit,
        DataMachine.AxisXPositiveLimit,
        DataMachine.AxisXNegativeLimit,
        DataMachine.AxisYPositiveLimit,
        DataMachine.AxisYNegativeLimit,
        DataMachine.AxisZPositiveLimit,
        DataMachine.AxisZNegativeLimit,
        DataMachine.AxisZPickDownSpeed,
        DataMachine.AxisZPickSpeed,
        DataMachine.AxisZDropSpeed,
        DataMachine.AxisXAutoSpeed,
        DataMachine.AxisYAutoSpeed,
        DataMachine.AxisZAutoSpeed,
        DataMachine.WaitPickPositionX,
        DataMachine.WaitPickPositionY,
        DataMachine.WaitPickPositionZ,
        DataMachine.SafeRotatePositionZ,
        DataMachine.SafeRotatePositionX,
        DataMachine.SafeRotatePositionZLower,
        DataMachine.PlaceProductLine1X,
        DataMachine.PlaceProductLine1Y,
        DataMachine.PlaceProductLine1Z,
        DataMachine.PlaceProductLine2X,
        DataMachine.PlaceProductLine2Y,
        DataMachine.PlaceProductLine2Z,
        DataMachine.MasterShaftOp1Diameter,
        DataMachine.AxisXTorqueUpperLimit,
        DataMachine.AxisXTorqueLowerLimit,
        DataMachine.AxisYTorqueUpperLimit,
        DataMachine.AxisYTorqueLowerLimit,
        DataMachine.AxisZTorqueUpperLimit,
        DataMachine.AxisZTorqueLowerLimit,
        DataOriginCheck.MoveToOriginCheckSpeedLimit,
        DataOriginCheck.OriginCheckSpeedLimit,
        DataOriginCheck.OffsetX,
        DataOriginCheck.OffsetY,
        DataOriginCheck.OffsetZ,
        DataOriginCheck.JigClampThickness,
        DataOriginCheck.OriginXLine1,
        DataOriginCheck.OriginYLine1,
        DataOriginCheck.OriginZLine1,
        DataOriginCheck.OriginXLine2,
        DataOriginCheck.OriginYLine2,
        DataOriginCheck.OriginZLine2,
        Manual.MoveXForward,
        Manual.MoveXBackward,
        Manual.HomeX,
        Manual.MoveXToPoint,
        Manual.ManualSpeedX,
        Manual.MovePointX,
        Manual.CurrentPositionX,
        Manual.IsHomingX,
        Manual.IsHomedX,
        Manual.MoveYLeft,
        Manual.MoveYRight,
        Manual.HomeY,
        Manual.MoveYToPoint,
        Manual.ManualSpeedY,
        Manual.MovePointY,
        Manual.CurrentPositionY,
        Manual.IsHomingY,
        Manual.IsHomedY,
        Manual.MoveZUp,
        Manual.MoveZDown,
        Manual.HomeZ,
        Manual.MoveZToPoint,
        Manual.ManualSpeedZ,
        Manual.MovePointZ,
        Manual.CurrentPositionZ,
        Manual.IsHomingZ,
        Manual.IsHomedZ,
        Manual.ToolClampIn,
        Manual.ToolClampOut,
        Manual.ToolRotate0,
        Manual.ToolRotate90,
        Manual.ClampCart1,
        Manual.UnclampCart1,
        Manual.ClampCart2,
        Manual.UnclampCart2,
        Manual.HomeAll,
        Manual.HomeRotateCylinder,
        Manual.HomeToolClampCylinder,
        Manual.BuzzerOnOff,
        Manual.LightCurtainOnOff,
        Manual.Cart1OpenedSignal,
        Manual.Cart1ClosedSignal,
        Manual.Cart2OpenedSignal,
        Manual.Cart2ClosedSignal,
        Manual.ToolClosedSignal,
        Manual.ToolOpenedSignal,
        Manual.RotatedTo0Signal,
        Manual.RotatedTo90Signal,
        Manual.HomeRotateCylinderDone,
        Manual.HomeToolClampDone,
        Manual.IsRunningToPointX,
        Manual.IsRunningToPointY,
        Manual.IsRunningToPointZ,
        Alarms.AlarmCode1,
        Alarms.AlarmCode2,
        Alarms.ErrorCode1,
        Alarms.ErrorCode2,
        Alarms.ErrorCode3,
        Alarms.ErrorCode4,
        Alarms.EStop,
        Alarms.XLimitNegative,
        Alarms.XLimitPositive,
        Alarms.YLimitNegative,
        Alarms.YLimitPositive,
        Alarms.ZLimitNegative,
        Alarms.ZLimitPositive,
        Alarms.PickSlip,
        Alarms.PlaceSlip,
        Alarms.CanOpenDisconnect,
        Alarms.LostPhase,
        Alarms.NotHomed,
        Alarms.AlarmDriverX,
        Alarms.AlarmDriverY,
        Alarms.AlarmDriverZ,
        Alarms.RotateCylinderTimeout0,
        Alarms.RotateCylinderTimeout90,
        Alarms.Cart1PositionInvalid,
        Alarms.Cart2PositionInvalid,
        Alarms.Cart1ClampCylinderFault,
        Alarms.Cart2ClampCylinderFault,
        Alarms.ToolGripperNoRelease,
        Alarms.ToolGripperNoClamp,
        Alarms.FailPlaceLine1,
        Alarms.FailPlaceLine2,
        Alarms.LightCurtain,
        Alarms.XSoftLimitOutside,
        Alarms.XSoftLimitInside,
        Alarms.YSoftLimitLeft,
        Alarms.YSoftLimitRight,
        Alarms.ZSoftLimitTop,
        Alarms.ZSoftLimitBottom,
        Alarms.HumanInWorkingZone,
        Alarms.AirPressureLost,
        Alarms.XOverMoment,
        Alarms.YOverMoment,
        Alarms.ZOverMoment,
        Alarms.PlcLine1Disconnected,
        Alarms.PlcLine2Disconnected,
        Alarms.PcDisconnected,
        Alarms.OrderNotEntered,
        Alarms.ProductParametersMissingLine1,
        Alarms.ProductParametersMissingLine2,
        Agv.MachineReadyForSwapLine1,
        Agv.MachineReadyForSwapLine2,
        Agv.AgvRequestSwapLine1,
        Agv.AgvRequestSwapLine2,
        Agv.AgvSwapDoneLine1,
        Agv.AgvSwapDoneLine2,
        ConfirmMessages.CollisionRotate90,
        ConfirmMessages.CollisionRotate0,
        ConfirmMessages.NotRotated90,
        ConfirmMessages.ZNotHomed,
        ConfirmMessages.OpenGripperBeforeHome,
        ConfirmMessages.ProductionCompleted,
    ];

    public static IReadOnlyDictionary<string, PlcTagDefinition> ByName { get; } =
        All.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> WritableTagNames { get; } = CollectTagNames(
        typeof(Words),
        typeof(Outputs),
        typeof(DataAutos),
        typeof(RobotTest),
        typeof(DataTrayCart),
        typeof(DataMachine),
        typeof(DataOriginCheck),
        typeof(Manual),
        typeof(Agv),
        typeof(ConfirmMessages));

    /// <summary>Tất cả tags của PLC Line 1.</summary>
    public static IReadOnlyList<PlcTagDefinition> AllLine1 { get; } = CollectAllTags(typeof(Line1));

    /// <summary>Tất cả tags của PLC Line 2.</summary>
    public static IReadOnlyList<PlcTagDefinition> AllLine2 { get; } = CollectAllTags(typeof(Line2));

    public static IReadOnlyDictionary<string, PlcTagDefinition> ByNameLine1 { get; } =
        CollectAllTags(typeof(Line1)).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, PlcTagDefinition> ByNameLine2 { get; } =
        CollectAllTags(typeof(Line2)).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    public static bool TryGet(string tagName, out PlcTagDefinition definition)
    {
        return ByName.TryGetValue(tagName, out definition!);
    }

    public static bool IsWritable(string tagName)
    {
        return WritableTagNames.Contains(tagName);
    }

    private static PlcTagDefinition CreateBit(string name, string address, string description)
    {
        return new PlcTagDefinition
        {
            Name = name,
            Address = address,
            DataType = PlcTagDataType.Bool,
            Description = description,
        };
    }

    private static PlcTagDefinition CreateWord(string name, string address, PlcTagDataType dataType, string description, int? lenght = null)
    {
        return new PlcTagDefinition
        {
            Name = name,
            Address = address,
            DataType = dataType,
            Description = description,
            Length = lenght,
        };
    }

    private static IReadOnlySet<string> CollectTagNames(params Type[] groupTypes)
    {
        return new HashSet<string>(
            groupTypes
                .SelectMany(static groupType => groupType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(static field => field.FieldType == typeof(PlcTagDefinition))
                .Select(static field => ((PlcTagDefinition?)field.GetValue(null))?.Name)
                .OfType<string>(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<PlcTagDefinition> CollectAllTags(params Type[] groupTypes)
    {
        return groupTypes
            .SelectMany(static groupType => groupType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(static field => field.FieldType == typeof(PlcTagDefinition))
            .Select(static field => (PlcTagDefinition?)field.GetValue(null))
            .OfType<PlcTagDefinition>()
            .ToList()
            .AsReadOnly();
    }
}

