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
            Description = "Word output group for Y0.00 - Y0.11",
        };

        public static readonly PlcTagDefinition D5131 = new()
        {
            Name = "word.d5131",
            Address = "D5131",
            DataType = PlcTagDataType.Int16,
            Description = "Word output group for Y1.00 - Y1.15",
        };

        public static readonly PlcTagDefinition D5132 = new()
        {
            Name = "word.d5132",
            Address = "D5132",
            DataType = PlcTagDataType.Int16,
            Description = "Word output group for Y2.00 - Y2.15",
        };

        public static readonly PlcTagDefinition D5133 = new()
        {
            Name = "word.d5133",
            Address = "D5133",
            DataType = PlcTagDataType.Int16,
            Description = "Word output group for Y3.00 - Y3.15",
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
        // --- Word 0: Y0.00 - Y0.11 ---
        public static readonly PlcTagDefinition Y0_00 = CreateBit("output.y0_00", "Y0.00", "Y0.0");
        public static readonly PlcTagDefinition Y0_01 = CreateBit("output.y0_01", "Y0.01", "Y0.1");
        public static readonly PlcTagDefinition Y0_02 = CreateBit("output.y0_02", "Y0.02", "Y0.2");
        public static readonly PlcTagDefinition Y0_03 = CreateBit("output.y0_03", "Y0.03", "Y0.3");
        public static readonly PlcTagDefinition Y0_04 = CreateBit("output.y0_04", "Y0.04", "Y0.4");
        public static readonly PlcTagDefinition Y0_05 = CreateBit("output.y0_05", "Y0.05", "Y0.5");
        public static readonly PlcTagDefinition Y0_06 = CreateBit("output.y0_06", "Y0.06", "Y0.6");
        public static readonly PlcTagDefinition Y0_07 = CreateBit("output.y0_07", "Y0.07", "XL Nâng Motor");
        public static readonly PlcTagDefinition Y0_08 = CreateBit("output.y0_08", "Y0.08", "XL Hạ Motor");
        public static readonly PlcTagDefinition Y0_09 = CreateBit("output.y0_09", "Y0.09", "XL Trước Nam Châm Cụm out ở trong");
        public static readonly PlcTagDefinition Y0_10 = CreateBit("output.y0_10", "Y0.10", "XL Trước Nam Châm Cụm out Ở ngoài");
        public static readonly PlcTagDefinition Y0_11 = CreateBit("output.y0_11", "Y0.11", "Dừng khẩn cấp");
        // --- Word 1: Y1.00 - Y1.15 ---
        public static readonly PlcTagDefinition Y1_00 = CreateBit("output.y1_00", "Y1.00", "Đèn tháp màu đỏ");
        public static readonly PlcTagDefinition Y1_01 = CreateBit("output.y1_01", "Y1.01", "Đèn tháp màu vàng");
        public static readonly PlcTagDefinition Y1_02 = CreateBit("output.y1_02", "Y1.02", "Đèn tháp màu xanh");
        public static readonly PlcTagDefinition Y1_03 = CreateBit("output.y1_03", "Y1.03", "Đèn tháp còi");
        public static readonly PlcTagDefinition Y1_04 = CreateBit("output.y1_04", "Y1.04", "Đèn nút Start");
        public static readonly PlcTagDefinition Y1_05 = CreateBit("output.y1_05", "Y1.05", "Đèn nút Stop");
        public static readonly PlcTagDefinition Y1_06 = CreateBit("output.y1_06", "Y1.06", "Đèn nút Reset");
        public static readonly PlcTagDefinition Y1_07 = CreateBit("output.y1_07", "Y1.07", "Đèn nút về gốc");
        public static readonly PlcTagDefinition Y1_08 = CreateBit("output.y1_08", "Y1.08", "XL kẹp xe");
        public static readonly PlcTagDefinition Y1_09 = CreateBit("output.y1_09", "Y1.09", "XL mở kẹp xe");
        public static readonly PlcTagDefinition Y1_10 = CreateBit("output.y1_10", "Y1.10", "XL đầu vào xoay 0");
        public static readonly PlcTagDefinition Y1_11 = CreateBit("output.y1_11", "Y1.11", "XL đầu vào xoay 90");
        public static readonly PlcTagDefinition Y1_12 = CreateBit("output.y1_12", "Y1.12", "XL đầu vào kẹp phôi");
        public static readonly PlcTagDefinition Y1_13 = CreateBit("output.y1_13", "Y1.13", "XL đầu vào mở kẹp phôi");
        public static readonly PlcTagDefinition Y1_14 = CreateBit("output.y1_14", "Y1.14", "Bàn xoay quay thuận");
        public static readonly PlcTagDefinition Y1_15 = CreateBit("output.y1_15", "Y1.15", "bàn Xoay quay nghịch");
        // --- Word 2: Y2.00 - Y2.15 ---
        public static readonly PlcTagDefinition Y2_00 = CreateBit("output.y2_00", "Y2.00", "Bàn xoay xóa lỗi");
        public static readonly PlcTagDefinition Y2_01 = CreateBit("output.y2_01", "Y2.01", "Bàn xoay chạy chậm");
        public static readonly PlcTagDefinition Y2_02 = CreateBit("output.y2_02", "Y2.02", "bàn xoay chạy nhanh");
        public static readonly PlcTagDefinition Y2_03 = CreateBit("output.y2_03", "Y2.03", "Cụm nâng đi lên");
        public static readonly PlcTagDefinition Y2_04 = CreateBit("output.y2_04", "Y2.04", "Cụm nâng đi xuống");
        public static readonly PlcTagDefinition Y2_05 = CreateBit("output.y2_05", "Y2.05", "Xóa Lỗi Cụm nâng");
        public static readonly PlcTagDefinition Y2_06 = CreateBit("output.y2_06", "Y2.06", "Cụm nâng chạy chậm");
        public static readonly PlcTagDefinition Y2_07 = CreateBit("output.y2_07", "Y2.07", "Cụm nâng chạy nhanh");
        public static readonly PlcTagDefinition Y2_08 = CreateBit("output.y2_08", "Y2.08", "Tay tool Xoay 0");
        public static readonly PlcTagDefinition Y2_09 = CreateBit("output.y2_09", "Y2.09", "Tay tool Xoay 90");
        public static readonly PlcTagDefinition Y2_10 = CreateBit("output.y2_10", "Y2.10", "Xì khí tay tool 1");
        public static readonly PlcTagDefinition Y2_11 = CreateBit("output.y2_11", "Y2.11", "Tay Tool Nam châm 1");
        public static readonly PlcTagDefinition Y2_12 = CreateBit("output.y2_12", "Y2.12", "Tay Tool Nam châm 2");
        public static readonly PlcTagDefinition Y2_13 = CreateBit("output.y2_13", "Y2.13", "Tay Tool Nam châm 3");
        public static readonly PlcTagDefinition Y2_14 = CreateBit("output.y2_14", "Y2.14", "Tay Tool Nam châm 4");
        public static readonly PlcTagDefinition Y2_15 = CreateBit("output.y2_15", "Y2.15", "Xì khí tay tool 2");
        // --- Word 3: Y3.00 - Y3.15 ---
        public static readonly PlcTagDefinition Y3_00 = CreateBit("output.y3_00", "Y3.00", "Kẹp phôi trước Hope");
        public static readonly PlcTagDefinition Y3_01 = CreateBit("output.y3_01", "Y3.01", "Mở kẹp phôi trước Hope");
        public static readonly PlcTagDefinition Y3_02 = CreateBit("output.y3_02", "Y3.02", "Cụm Output xoay 0");
        public static readonly PlcTagDefinition Y3_03 = CreateBit("output.y3_03", "Y3.03", "Cụm Output xoay 180");
        public static readonly PlcTagDefinition Y3_04 = CreateBit("output.y3_04", "Y3.04", "XL chuyển hàng X vào trong");
        public static readonly PlcTagDefinition Y3_05 = CreateBit("output.y3_05", "Y3.05", "XL chuyển hàng X ra ngoài");
        public static readonly PlcTagDefinition Y3_06 = CreateBit("output.y3_06", "Y3.06", "XL chuyển hàng Y vào trong");
        public static readonly PlcTagDefinition Y3_07 = CreateBit("output.y3_07", "Y3.07", "XL chuyển hàng Y ra ngoài");
        public static readonly PlcTagDefinition Y3_08 = CreateBit("output.y3_08", "Y3.08", "Cụm Output nam châm1");
        public static readonly PlcTagDefinition Y3_09 = CreateBit("output.y3_09", "Y3.09", "Cụm Output nam châm2");
        public static readonly PlcTagDefinition Y3_10 = CreateBit("output.y3_10", "Y3.10", "Phanh Bàn Xoay");
        public static readonly PlcTagDefinition Y3_11 = CreateBit("output.y3_11", "Y3.11", "Phanh Cụm Nâng");
        public static readonly PlcTagDefinition Y3_12 = CreateBit("output.y3_12", "Y3.12", "Máy tiện 1 Mở/kẹp Chấu cặp");
        public static readonly PlcTagDefinition Y3_13 = CreateBit("output.y3_13", "Y3.13", "Máy tiện 1 Chạy");
        public static readonly PlcTagDefinition Y3_14 = CreateBit("output.y3_14", "Y3.14", "Máy tiện 2 Mở/kẹp Chấu cặp");
        public static readonly PlcTagDefinition Y3_15 = CreateBit("output.y3_15", "Y3.15", "Máy tiện 2 Chạy");
        // Aliases for backward compatibility
        public static readonly PlcTagDefinition Y0_09AxisXOn = Y0_09;
        public static readonly PlcTagDefinition Y0_10AxisYOn = Y0_10;
        public static readonly PlcTagDefinition Y0_11AxisZOn = Y0_11;
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
        public static readonly PlcTagDefinition AxisXSpeedLimit = CreateWord("data.axis_x_speed_limit", "D21000", PlcTagDataType.Float, "Giới hạn tốc độ trục X");
        public static readonly PlcTagDefinition AxisZSpeedLimit = CreateWord("data.axis_z_speed_limit", "D21002", PlcTagDataType.Float, "Giới hạn tốc độ trục Z");
        public static readonly PlcTagDefinition LifterSpeedLimit = CreateWord("data.lifter_speed_limit", "D21004", PlcTagDataType.Float, "Giới hạn Tốc độ trục cấp phôi");
        public static readonly PlcTagDefinition RotarySpeedLimit = CreateWord("data.rotary_speed_limit", "D21006", PlcTagDataType.Float, "Giới hạn Tốc độ bàn xoay");
        public static readonly PlcTagDefinition AxisXPositiveLimit = CreateWord("data.axis_x_positive_limit", "D21008", PlcTagDataType.Float, "Limit trục X+");
        public static readonly PlcTagDefinition AxisXNegativeLimit = CreateWord("data.axis_x_negative_limit", "D21010", PlcTagDataType.Float, "Limit trục X-");
        public static readonly PlcTagDefinition AxisZPositiveLimit = CreateWord("data.axis_z_positive_limit", "D21012", PlcTagDataType.Float, "Limit trục Z+");
        public static readonly PlcTagDefinition AxisZNegativeLimit = CreateWord("data.axis_z_negative_limit", "D21014", PlcTagDataType.Float, "Limit trục Z-");
        public static readonly PlcTagDefinition LifterTopLimit = CreateWord("data.lifter_top_limit", "D21016", PlcTagDataType.Float, "Limit trên trục nâng phôi");
        public static readonly PlcTagDefinition LifterBottomLimit = CreateWord("data.lifter_bottom_limit", "D21018", PlcTagDataType.Float, "Limit dưới trục nâng phôi");
        public static readonly PlcTagDefinition AxisXAutoSpeed = CreateWord("data.axis_x_auto_speed", "D21020", PlcTagDataType.Float, "Tốc độ chạy tự động trục X");
        public static readonly PlcTagDefinition AxisZAutoSpeed = CreateWord("data.axis_z_auto_speed", "D21022", PlcTagDataType.Float, "Tốc độ chạy tự động trục Z");
        public static readonly PlcTagDefinition LifterAutoSpeed = CreateWord("data.lifter_auto_speed", "D21024", PlcTagDataType.Float, "Tốc độ chạy tự động trục nâng phôi");
        public static readonly PlcTagDefinition RotaryAutoSpeed = CreateWord("data.rotary_auto_speed", "D21026", PlcTagDataType.Float, "Tốc độ chạy tự động bàn nâng");
        public static readonly PlcTagDefinition AxisXFeedFastSpeed = CreateWord("data.axis_x_feed_fast_speed", "D21028", PlcTagDataType.Float, "Tốc độ trục X cấp hàng đi nhanh");
        public static readonly PlcTagDefinition AxisXFeedSlowSpeed = CreateWord("data.axis_x_feed_slow_speed", "D21030", PlcTagDataType.Float, "Tốc độ trục X cấp hàng đi chậm");
        public static readonly PlcTagDefinition AxisXPickSpeed = CreateWord("data.axis_x_pick_speed", "D21032", PlcTagDataType.Float, "Tốc độ trục X lấy hàng");
        public static readonly PlcTagDefinition AxisZInMachineSpeed = CreateWord("data.axis_z_in_machine_speed", "D21034", PlcTagDataType.Float, "Tốc độ Z trong máy mài");
        public static readonly PlcTagDefinition AxisXTorqueUpperLimit = CreateWord("data.axis_x_torque_upper_limit", "D21036", PlcTagDataType.Int16, "Giới hạn trên Momen trục X");
        public static readonly PlcTagDefinition AxisXTorqueLowerLimit = CreateWord("data.axis_x_torque_lower_limit", "D21038", PlcTagDataType.Int16, "Giới hạn dưới Momen trục X");
        public static readonly PlcTagDefinition AxisZTorqueUpperLimit = CreateWord("data.axis_z_torque_upper_limit", "D21040", PlcTagDataType.Int16, "Giới hạn trên Momen trục Z");
        public static readonly PlcTagDefinition AxisZTorqueLowerLimit = CreateWord("data.axis_z_torque_lower_limit", "D21042", PlcTagDataType.Int16, "Giới hạn dưới Momen trục Z");
        public static readonly PlcTagDefinition AxisXOffset = CreateWord("data.axis_x_offset", "D21044", PlcTagDataType.Float, "Ofset tọa độ trục X");
        public static readonly PlcTagDefinition AxisZOffset = CreateWord("data.axis_z_offset", "D21046", PlcTagDataType.Float, "Ofset tọa độ trục Z");
        public static readonly PlcTagDefinition LifterOffset = CreateWord("data.lifter_offset", "D21048", PlcTagDataType.Float, "Ofset tọa độ trục nâng");
    }

    public static class DataMachine
    {
        // --- Tọa độ Master & Máy tiện OP1 / OP2 (Image 3) ---
        public static readonly PlcTagDefinition MasterPickInputX = CreateWord("data.master_pick_input_x", "D21060", PlcTagDataType.Float, "Tọa độ X Master lấy phôi đầu vào");
        public static readonly PlcTagDefinition MasterPickInputZ = CreateWord("data.master_pick_input_z", "D21062", PlcTagDataType.Float, "Tọa độ Z Master lấy phôi đầu vào");
        public static readonly PlcTagDefinition LatheOp1JawFaceX = CreateWord("data.lathe_op1_jaw_face_x", "D21064", PlcTagDataType.Float, "Tọa độ X Mặt chấu kẹp máy tiện OP1");
        public static readonly PlcTagDefinition LatheOp1SafeUpDownX = CreateWord("data.lathe_op1_safe_up_down_x", "D21066", PlcTagDataType.Float, "Tọa độ X An toàn lên xuống máy tiện OP1");
        public static readonly PlcTagDefinition LatheOp1JawCenterZ = CreateWord("data.lathe_op1_jaw_center_z", "D21068", PlcTagDataType.Float, "Tọa độ Z tâm chấu kẹp máy tiện OP1");
        public static readonly PlcTagDefinition LatheOp2JawFaceX = CreateWord("data.lathe_op2_jaw_face_x", "D21070", PlcTagDataType.Float, "Tọa độ X Mặt chấu kẹp máy tiện OP2");
        public static readonly PlcTagDefinition LatheOp2SafeUpDownX = CreateWord("data.lathe_op2_safe_up_down_x", "D21072", PlcTagDataType.Float, "Tọa độ X An toàn lên xuống máy tiện OP2");
        public static readonly PlcTagDefinition LatheOp2JawCenterZ = CreateWord("data.lathe_op2_jaw_center_z", "D21074", PlcTagDataType.Float, "Tọa độ Z tâm chấu kẹp máy tiện OP2");
        public static readonly PlcTagDefinition MasterDropProductX = CreateWord("data.master_drop_product_x", "D21076", PlcTagDataType.Float, "Tọa độ X Master Thả hàng thành phẩm");
        public static readonly PlcTagDefinition MasterDropProductZ = CreateWord("data.master_drop_product_z", "D21078", PlcTagDataType.Float, "Tọa độ Z Master Thả hàng thành phẩm");
        public static readonly PlcTagDefinition MasterDropFlipX = CreateWord("data.master_drop_flip_x", "D21080", PlcTagDataType.Float, "Tọa độ X Master thả hàng đảo chiều phôi");
        public static readonly PlcTagDefinition MasterPickPostFlipX = CreateWord("data.master_pick_post_flip_x", "D21082", PlcTagDataType.Float, "Tọa độ X Master lấy hàng phôi sau đảo chiều");
        public static readonly PlcTagDefinition MasterDropFlipZ = CreateWord("data.master_drop_flip_z", "D21084", PlcTagDataType.Float, "Tọa độ Z Master thả hàng đảo chiều phôi");

        // --- Momen status readouts (Image 2) ---
        public static readonly PlcTagDefinition MomentXMin = CreateWord("data.moment_x_min", "D5190", PlcTagDataType.Int16, "Momen X Min");
        public static readonly PlcTagDefinition MomentXMax = CreateWord("data.moment_x_max", "D5191", PlcTagDataType.Int16, "Momen X Max");
        public static readonly PlcTagDefinition MomentZMin = CreateWord("data.moment_z_min", "D5192", PlcTagDataType.Int16, "Momen Z Min");
        public static readonly PlcTagDefinition MomentZMax = CreateWord("data.moment_z_max", "D5193", PlcTagDataType.Int16, "Momen Z Max");
    }


    public static class Manual
    {
        // --- Trục X ---
        public static readonly PlcTagDefinition MoveXForward = CreateBit("manual.move_x_forward", "M2000", "Tiến trục X");
        public static readonly PlcTagDefinition MoveXBackward = CreateBit("manual.move_x_backward", "M2001", "Lùi trục X");
        public static readonly PlcTagDefinition HomeX = CreateBit("manual.home_x", "M2002", "Về gốc trục X");
        public static readonly PlcTagDefinition MoveXToPoint = CreateBit("manual.move_x_to_point", "M2003", "Chạy điểm vị trí trục X");
        public static readonly PlcTagDefinition ManualSpeedX = CreateWord("manual.manual_speed_x", "D5150", PlcTagDataType.Float, "Tốc độ Manual trục X (mm/s)");
        public static readonly PlcTagDefinition MovePointX = CreateWord("manual.move_point_x", "D5152", PlcTagDataType.Float, "Vị trí chạy điểm (mm)");
        public static readonly PlcTagDefinition CurrentPositionX = CreateWord("manual.current_position_x", "D5154", PlcTagDataType.Float, "Vị trí hiện tại trục X (hiển thị) (mm)");
        public static readonly PlcTagDefinition IsHomingX = CreateBit("manual.is_homing_x", "M2050", "Đang về Home X");
        public static readonly PlcTagDefinition IsHomedX = CreateBit("manual.is_homed_x", "M2051", "Đã về Home X");
        public static readonly PlcTagDefinition IsRunningToPointX = CreateBit("manual.is_running_to_point_x", "M2052", "Đang chạy điểm trục X");

        // --- Trục Z ---
        public static readonly PlcTagDefinition MoveZRight = CreateBit("manual.move_z_right", "M2004", "Phải trục Z");
        public static readonly PlcTagDefinition MoveZLeft = CreateBit("manual.move_z_left", "M2005", "Trái trục Z");
        public static readonly PlcTagDefinition HomeZ = CreateBit("manual.home_z", "M2006", "Về gốc trục Z");
        public static readonly PlcTagDefinition MoveZToPoint = CreateBit("manual.move_z_to_point", "M2007", "Chạy điểm vị trí trục Z");
        public static readonly PlcTagDefinition ManualSpeedZ = CreateWord("manual.manual_speed_z", "D5156", PlcTagDataType.Float, "Tốc độ Manual trục Z (mm/s)");
        public static readonly PlcTagDefinition MovePointZ = CreateWord("manual.move_point_z", "D5158", PlcTagDataType.Float, "Vị trí chạy điểm trục Z( mm )");
        public static readonly PlcTagDefinition CurrentPositionZ = CreateWord("manual.current_position_z", "D5160", PlcTagDataType.Float, "Vị trí hiện tại trục Z ( hiển thị ) ( mm )");
        public static readonly PlcTagDefinition IsHomingZ = CreateBit("manual.is_homing_z", "M2054", "Đang về Home Z");
        public static readonly PlcTagDefinition IsHomedZ = CreateBit("manual.is_homed_z", "M2055", "Đã về Home Z");
        public static readonly PlcTagDefinition IsRunningToPointZ = CreateBit("manual.is_running_to_point_z", "M2056", "Đang chạy điểm trục Z");

        // --- Trục Nâng ---
        public static readonly PlcTagDefinition MoveLifterUp = CreateBit("manual.move_lifter_up", "M2008", "lên trục cấp phôi");
        public static readonly PlcTagDefinition MoveLifterDown = CreateBit("manual.move_lifter_down", "M2009", "Xuống trục cấp phôi");
        public static readonly PlcTagDefinition HomeLifter = CreateBit("manual.home_lifter", "M2010", "Về gốc trục cấp phôi");
        public static readonly PlcTagDefinition MoveLifterToPoint = CreateBit("manual.move_lifter_to_point", "M2011", "Chạy điểm vị trí trục cấp phôi");
        public static readonly PlcTagDefinition ManualSpeedLifter = CreateWord("manual.manual_speed_lifter", "D5162", PlcTagDataType.Float, "Tốc độ Manual trục cấp phôi ( mm/s)( Không cho nhập, mặc định)");
        public static readonly PlcTagDefinition MovePointLifter = CreateWord("manual.move_point_lifter", "D5164", PlcTagDataType.Float, "Vị trí chạy điểm trục cấp phôi( mm )");
        public static readonly PlcTagDefinition CurrentPositionLifter = CreateWord("manual.current_position_lifter", "D5166", PlcTagDataType.Float, "Vị trí hiện tại trục cấp phôi ( hiển thị ) ( mm )");
        public static readonly PlcTagDefinition IsHomingLifter = CreateBit("manual.is_homing_lifter", "M2058", "Đang về Home trục cấp phôi");
        public static readonly PlcTagDefinition IsHomedLifter = CreateBit("manual.is_homed_lifter", "M2059", "Đã về Home trục cấp phôi");
        public static readonly PlcTagDefinition IsRunningToPointLifter = CreateBit("manual.is_running_to_point_lifter", "M2060", "Đang chạy điểm trục cấp phôi");

        // --- Bàn xoay ---
        public static readonly PlcTagDefinition MoveRotaryForward = CreateBit("manual.move_rotary_forward", "M2012", "Quay thuận bàn xoay");
        public static readonly PlcTagDefinition MoveRotaryReverse = CreateBit("manual.move_rotary_reverse", "M2013", "Quay nghịch bàn xoay");
        public static readonly PlcTagDefinition HomeRotary = CreateBit("manual.home_rotary", "M2014", "Về gốc bàn xoay");
        public static readonly PlcTagDefinition MoveRotaryToPoint = CreateBit("manual.move_rotary_to_point", "M2015", "Chạy điểm bàn xoay");
        public static readonly PlcTagDefinition ManualSpeedRotary = CreateWord("manual.manual_speed_rotary", "D5168", PlcTagDataType.Float, "Tốc độ Manual bàn xoay ( không cho nhập, mặc định)");
        public static readonly PlcTagDefinition MovePointRotary = CreateWord("manual.move_point_rotary", "D5170", PlcTagDataType.Float, "Vị trí chạy điểm Góc bàn xoay( mm )0,90,180,270");
        public static readonly PlcTagDefinition CurrentPositionRotary = CreateWord("manual.current_position_rotary", "D5172", PlcTagDataType.Float, "Vị trí hiện tại bàn xoay");
        public static readonly PlcTagDefinition IsHomingRotary = CreateBit("manual.is_homing_rotary", "M2062", "Đang về Home bàn xoay");
        public static readonly PlcTagDefinition IsHomedRotary = CreateBit("manual.is_homed_rotary", "M2063", "Đã về Home bàn xoay");
        public static readonly PlcTagDefinition IsRunningToPointRotary = CreateBit("manual.is_running_to_point_rotary", "M2064", "Đang chạy điểm bàn xoay");

        // --- Xilanh Kẹp Xe ---
        public static readonly PlcTagDefinition ClampCart = CreateBit("manual.clamp_cart", "M2016", "Kẹp Xilanh Kẹp Xe");
        public static readonly PlcTagDefinition UnclampCart = CreateBit("manual.unclamp_cart", "M2017", "Mở Xilanh Kẹp Xe");
        public static readonly PlcTagDefinition CartClampedSignal = CreateBit("manual.cart_clamped_signal", "M2066", "Đã kẹp xe");
        public static readonly PlcTagDefinition CartUnclampedSignal = CreateBit("manual.cart_unclamped_signal", "M2067", "Đã mở kẹp xe");

        // --- Xilanh Nâng Động cơ bàn xoay ---
        public static readonly PlcTagDefinition LiftMotorUp = CreateBit("manual.lift_motor_up", "M2018", "Nâng Xilanh động cơ bàn xoay");
        public static readonly PlcTagDefinition LiftMotorDown = CreateBit("manual.lift_motor_down", "M2019", "Hạ Xilanh động cơ bàn xoay");
        public static readonly PlcTagDefinition LiftMotorUpSignal = CreateBit("manual.lift_motor_up_signal", "M2068", "Đã nâng");
        public static readonly PlcTagDefinition LiftMotorDownSignal = CreateBit("manual.lift_motor_down_signal", "M2069", "Đã Hạ");

        // --- Xianh kẹp phôi đầu vào ---
        public static readonly PlcTagDefinition InputClampPart = CreateBit("manual.input_clamp_part", "M2020", "Kẹp Xianh kẹp phôi đầu vào");
        public static readonly PlcTagDefinition InputUnclampPart = CreateBit("manual.input_unclamp_part", "M2021", "Mở Xilanh Kẹp phôi đầu vào");
        public static readonly PlcTagDefinition InputClampedSignal = CreateBit("manual.input_clamped_signal", "M2070", "Đã kẹp");
        public static readonly PlcTagDefinition InputUnclampedSignal = CreateBit("manual.input_unclamped_signal", "M2071", "Đã mở kẹp");

        // --- Xilanh Lật phôi đầu vào ---
        public static readonly PlcTagDefinition InputRotate0 = CreateBit("manual.input_rotate_0", "M2022", "Xoay 0 Xilanh Lật phôi đầu vào");
        public static readonly PlcTagDefinition InputRotate90 = CreateBit("manual.input_rotate_90", "M2023", "Xoay 90 Xilanh lật phôi đầu vào");
        public static readonly PlcTagDefinition InputRotated0Signal = CreateBit("manual.input_rotated_0_signal", "M2072", "Đã xoay 0");
        public static readonly PlcTagDefinition InputRotated90Signal = CreateBit("manual.input_rotated_90_signal", "M2073", "Đã xoay 90");

        // --- Tay cấp phôi Rodal ---
        public static readonly PlcTagDefinition RodalRotate0 = CreateBit("manual.rodal_rotate_0", "M2024", "Xoay 0 Cụm cấp phôi Rodal");
        public static readonly PlcTagDefinition RodalRotate180 = CreateBit("manual.rodal_rotate_180", "M2025", "Xoay 180 Cụm cấp phôi Rodal");
        public static readonly PlcTagDefinition RodalRotated0Signal = CreateBit("manual.rodal_rotated_0_signal", "M2074", "Đã xoay 0");
        public static readonly PlcTagDefinition RodalRotated180Signal = CreateBit("manual.rodal_rotated_180_signal", "M2075", "Đã xoay 180");

        // --- Xilanh lật sau máy tiện 2 ---
        public static readonly PlcTagDefinition Lathe2FlipRotate0 = CreateBit("manual.lathe2_flip_rotate_0", "M2026", "Quay 0 Xilanh lật sau máy tiện 2");
        public static readonly PlcTagDefinition Lathe2FlipRotate90 = CreateBit("manual.lathe2_flip_rotate_90", "M2027", "Quay 90 xilanh lật sau máy tiện 2");
        public static readonly PlcTagDefinition Lathe2FlipRotated0Signal = CreateBit("manual.lathe2_flip_rotated_0_signal", "M2076", "Đã xoay 0");
        public static readonly PlcTagDefinition Lathe2FlipRotated90Signal = CreateBit("manual.lathe2_flip_rotated_90_signal", "M2077", "đã quay 90");

        // --- Xilanh tranfer sau tiện 2 ---
        public static readonly PlcTagDefinition Lathe2TransferOut = CreateBit("manual.lathe2_transfer_out", "M2028", "Đi Ra--Xilanh tranfer sau tiện 2");
        public static readonly PlcTagDefinition Lathe2TransferIn = CreateBit("manual.lathe2_transfer_in", "M2029", "Đi vào-- Xilanh tranfer sau tiện 2");
        public static readonly PlcTagDefinition Lathe2TransferOutSignal = CreateBit("manual.lathe2_transfer_out_signal", "M2078", "Đã ra");
        public static readonly PlcTagDefinition Lathe2TransferInSignal = CreateBit("manual.lathe2_transfer_in_signal", "M2079", "Đã vào");

        // --- Xilanh Out phôi thành phẩm ---
        public static readonly PlcTagDefinition ProductOutExtend = CreateBit("manual.product_out_extend", "M2030", "Đi Ra-- Xilanh Out phôi thành phẩm");
        public static readonly PlcTagDefinition ProductOutRetract = CreateBit("manual.product_out_retract", "M2031", "Đi vào-- Xilanh Out phôi thành phẩm");
        public static readonly PlcTagDefinition ProductOutExtendedSignal = CreateBit("manual.product_out_extended_signal", "M2080", "Đã ra");
        public static readonly PlcTagDefinition ProductOutRetractedSignal = CreateBit("manual.product_out_retracted_signal", "M2081", "Đã Vào");

        // --- Xilanh kẹp phôi thành phẩm ---
        public static readonly PlcTagDefinition ProductClampPart = CreateBit("manual.product_clamp_part", "M2032", "Kẹp Xilanh kẹp phôi thành phẩm");
        public static readonly PlcTagDefinition ProductUnclampPart = CreateBit("manual.product_unclamp_part", "M2033", "Mở Xilanh kẹp phôi thành phẩm");
        public static readonly PlcTagDefinition ProductClampedSignal = CreateBit("manual.product_clamped_signal", "M2082", "Đã Kẹp");
        public static readonly PlcTagDefinition ProductUnclampedSignal = CreateBit("manual.product_unclamped_signal", "M2083", "Đã mở kẹp");

        // --- Tắt Còi ---
        public static readonly PlcTagDefinition BuzzerOff = CreateBit("manual.buzzer_off", "M2090", "Tắt Còi");

        // --- Backward Compatibility Aliases ---
        public static readonly PlcTagDefinition MoveYLeft = MoveZLeft;
        public static readonly PlcTagDefinition MoveYRight = MoveZRight;
        public static readonly PlcTagDefinition HomeY = HomeZ;
        public static readonly PlcTagDefinition MoveYToPoint = MoveZToPoint;
        public static readonly PlcTagDefinition ManualSpeedY = ManualSpeedZ;
        public static readonly PlcTagDefinition MovePointY = MovePointZ;
        public static readonly PlcTagDefinition CurrentPositionY = CurrentPositionZ;
        public static readonly PlcTagDefinition IsHomingY = IsHomingZ;
        public static readonly PlcTagDefinition IsHomedY = IsHomedZ;
        public static readonly PlcTagDefinition MoveZUp = MoveLifterUp;
        public static readonly PlcTagDefinition MoveZDown = MoveLifterDown;
        public static readonly PlcTagDefinition ToolClampIn = InputClampPart;
        public static readonly PlcTagDefinition ToolClampOut = InputUnclampPart;
        public static readonly PlcTagDefinition ToolRotate0 = InputRotate0;
        public static readonly PlcTagDefinition ToolRotate90 = InputRotate90;
        public static readonly PlcTagDefinition ClampCart1 = ClampCart;
        public static readonly PlcTagDefinition UnclampCart1 = UnclampCart;
        public static readonly PlcTagDefinition ClampCart2 = ClampCart;
        public static readonly PlcTagDefinition UnclampCart2 = UnclampCart;
        public static readonly PlcTagDefinition HomeAll = HomeX;
        public static readonly PlcTagDefinition HomeRotateCylinder = InputRotate0;
        public static readonly PlcTagDefinition HomeToolClampCylinder = InputClampPart;
        public static readonly PlcTagDefinition BuzzerOnOff = BuzzerOff;
        public static readonly PlcTagDefinition LightCurtainOnOff = BuzzerOff;
        public static readonly PlcTagDefinition Cart1OpenedSignal = CartUnclampedSignal;
        public static readonly PlcTagDefinition Cart1ClosedSignal = CartClampedSignal;
        public static readonly PlcTagDefinition Cart2OpenedSignal = CartUnclampedSignal;
        public static readonly PlcTagDefinition Cart2ClosedSignal = CartClampedSignal;
        public static readonly PlcTagDefinition ToolClosedSignal = InputClampedSignal;
        public static readonly PlcTagDefinition ToolOpenedSignal = InputUnclampedSignal;
        public static readonly PlcTagDefinition RotatedTo0Signal = InputRotated0Signal;
        public static readonly PlcTagDefinition RotatedTo90Signal = InputRotated90Signal;
        public static readonly PlcTagDefinition HomeRotateCylinderDone = InputRotated0Signal;
        public static readonly PlcTagDefinition HomeToolClampDone = InputClampedSignal;
        public static readonly PlcTagDefinition IsRunningToPointY = IsRunningToPointZ;
    }

      public static class Alarms
    {
        public static readonly PlcTagDefinition AlarmCode1 = CreateWord("alarm.code_1", "D5140", PlcTagDataType.Int16, "Mã cảnh báo 1");
        public static readonly PlcTagDefinition AlarmCode2 = CreateWord("alarm.code_2", "D5141", PlcTagDataType.Int16, "Mã cảnh báo 2");
        public static readonly PlcTagDefinition ErrorCode1 = CreateWord("alarm.error_code_1", "D5142", PlcTagDataType.Int16, "Mã Lỗi 1");
        public static readonly PlcTagDefinition ErrorCode2 = CreateWord("alarm.error_code_2", "D5143", PlcTagDataType.Int16, "Mã Lỗi 2");
        public static readonly PlcTagDefinition ErrorCode3 = CreateWord("alarm.error_code_3", "D5144", PlcTagDataType.Int16, "Mã Lỗi 3");
        public static readonly PlcTagDefinition ErrorCode4 = CreateWord("alarm.error_code_4", "D5145", PlcTagDataType.Int16, "Mã Lỗi 4");
        public static readonly PlcTagDefinition ErrorCode5 = CreateWord("alarm.error_code_5", "D5146", PlcTagDataType.Int16, "Mã Lỗi 5");
        public static readonly PlcTagDefinition ErrorCode6 = CreateWord("alarm.error_code_6", "D5147", PlcTagDataType.Int16, "Mã Lỗi 6");
        public static readonly PlcTagDefinition ErrorCode7 = CreateWord("alarm.error_code_7", "D5148", PlcTagDataType.Int16, "Mã Lỗi 7");

        // D5140
        public static readonly PlcTagDefinition EStop = CreateBit("alarm.estop", "D5140.0", "Lỗi dừng khẩn cấp");

        // D5141
        public static readonly PlcTagDefinition XSoftLimitOutside = CreateBit("alarm.x_soft_limit_outside", "D5141.0", "Lỗi giới hạn mềm ngoài trục X");
        public static readonly PlcTagDefinition XSoftLimitInside = CreateBit("alarm.x_soft_limit_inside", "D5141.1", "Lỗi giới hạn mềm trong trục X");
        public static readonly PlcTagDefinition ZSoftLimitBottom = CreateBit("alarm.z_soft_limit_bottom", "D5141.2", "Lỗi giới hạn mềm dưới trục Z");
        public static readonly PlcTagDefinition ZSoftLimitTop = CreateBit("alarm.z_soft_limit_top", "D5141.3", "Lỗi giới hạn mềm trên trục Z");
        public static readonly PlcTagDefinition NotHomedRodal = CreateBit("alarm.not_homed_rodal", "D5141.4", "Lỗi chưa về gốc Rodal");
        public static readonly PlcTagDefinition AlarmDriverX = CreateBit("alarm.alarm_driver_x", "D5141.5", "Lỗi Driver trục X");
        public static readonly PlcTagDefinition AlarmDriverZ = CreateBit("alarm.alarm_driver_z", "D5141.6", "Lỗi Driver trục Z");

        // D5142
        public static readonly PlcTagDefinition XLimitNegative = CreateBit("alarm.x_limit_negative", "D5142.0", "Lỗi giới hạn cứng ngoài trục X");
        public static readonly PlcTagDefinition XLimitPositive = CreateBit("alarm.x_limit_positive", "D5142.1", "Lỗi giới hạn cứng trong trục X");
        public static readonly PlcTagDefinition ZLimitNegative = CreateBit("alarm.z_limit_negative", "D5142.2", "Lỗi giới hạn cứng dưới trục Z");
        public static readonly PlcTagDefinition ZLimitPositive = CreateBit("alarm.z_limit_positive", "D5142.3", "Lỗi Giới hạn cứng trên trục Z");
        public static readonly PlcTagDefinition XOverMoment = CreateBit("alarm.x_over_moment", "D5142.4", "Lỗi quá Momen trục X");
        public static readonly PlcTagDefinition ZOverMoment = CreateBit("alarm.z_over_moment", "D5142.5", "Lỗi quá Momen trục Z");
        public static readonly PlcTagDefinition AirPressureLost = CreateBit("alarm.air_pressure_lost", "D5142.6", "Lỗi áp suất khí nén mức thấp");
        public static readonly PlcTagDefinition RotateCylinderTimeout0 = CreateBit("alarm.rotate_cylinder_timeout_0", "D5142.7", "Lỗi TimeOut Xilanh xoay 0 cụm Tool");
        public static readonly PlcTagDefinition RotateCylinderTimeout180 = CreateBit("alarm.rotate_cylinder_timeout_180", "D5142.8", "Lỗi TimeOut Xilanh xoay 180 cụm Tool");
        public static readonly PlcTagDefinition ToolCheckSensor1 = CreateBit("alarm.tool_check_sensor_1", "D5142.9", "Lỗi cảm biến check hàng tool 1");
        public static readonly PlcTagDefinition ToolCheckSensor2 = CreateBit("alarm.tool_check_sensor_2", "D5142.10", "Lỗi cảm biến check hàng tool 2");

        // D5143
        public static readonly PlcTagDefinition LifterSoftLimitBottom = CreateBit("alarm.lifter_soft_limit_bottom", "D5143.0", "Lỗi giới hạn mềm dưới cụm nâng");
        public static readonly PlcTagDefinition LifterSoftLimitTop = CreateBit("alarm.lifter_soft_limit_top", "D5143.1", "Lỗi giới hạn mềm trên cụm nâng");
        public static readonly PlcTagDefinition LifterInverterFault = CreateBit("alarm.lifter_inverter_fault", "D5143.2", "Lỗi biến tần cụm nâng");
        public static readonly PlcTagDefinition RotaryInverterFault = CreateBit("alarm.rotary_inverter_fault", "D5143.3", "Lỗi biến tần bàn xoay");
        public static readonly PlcTagDefinition InputGroupNotHomed = CreateBit("alarm.input_group_not_homed", "D5143.4", "Lỗi chưa về gốc Cụm Input");
        public static readonly PlcTagDefinition LifterPulseSlip = CreateBit("alarm.lifter_pulse_slip", "D5143.5", "Lỗi trượt xung cụm nâng");
        public static readonly PlcTagDefinition RotaryPulseSlip = CreateBit("alarm.rotary_pulse_slip", "D5143.6", "Lỗi trượt xung bàn xoay");

        // D5144
        public static readonly PlcTagDefinition LifterHardLimitBottom = CreateBit("alarm.lifter_hard_limit_bottom", "D5144.0", "Lỗi giới hạn cứng dưới cụm nâng");
        public static readonly PlcTagDefinition LifterHardLimitTop = CreateBit("alarm.lifter_hard_limit_top", "D5144.1", "Lỗi giới hạn cứng trên cụm nâng");
        public static readonly PlcTagDefinition CartClampTimeout = CreateBit("alarm.cart_clamp_timeout", "D5144.2", "Lỗi Timeout Xilanh kẹp xe");
        public static readonly PlcTagDefinition CartUnclampTimeout = CreateBit("alarm.cart_unclamp_timeout", "D5144.3", "Lỗi Timeout Xilanh mở kẹp xe");
        public static readonly PlcTagDefinition CartPositionInvalid = CreateBit("alarm.cart_position_invalid", "D5144.4", "Lôi sai vị trí xe");
        public static readonly PlcTagDefinition RotaryNotAtHome = CreateBit("alarm.rotary_not_at_home", "D5144.5", "Lỗi Bàn xoay không ở gốc");
        public static readonly PlcTagDefinition MotorNotAtHome = CreateBit("alarm.motor_not_at_home", "D5144.6", "Lỗi Motor không ở gốc");
        public static readonly PlcTagDefinition InputRotateTimeout0 = CreateBit("alarm.input_rotate_timeout_0", "D5144.7", "Lỗi TimeOut XL xoay 0 cụm input");
        public static readonly PlcTagDefinition InputRotateTimeout90 = CreateBit("alarm.input_rotate_timeout_90", "D5144.8", "Lỗi TimeOut XL xoay 90 cụm input");
        public static readonly PlcTagDefinition InputClampTimeout = CreateBit("alarm.input_clamp_timeout", "D5144.9", "Lỗi Timout kẹp phôi cụm Input");
        public static readonly PlcTagDefinition InputUnclampTimeout = CreateBit("alarm.input_unclamp_timeout", "D5144.10", "Lỗi Timout mở kẹp phôi cụm Input");
        public static readonly PlcTagDefinition InputCheckSensorFault = CreateBit("alarm.input_check_sensor_fault", "D5144.11", "Lỗi cảm biến check hàng đầu vào");

        // D5147
        public static readonly PlcTagDefinition OutputRotateTimeout0 = CreateBit("alarm.output_rotate_timeout_0", "D5147.0", "Lỗi TimeOut Xilanh xoay 0 Cụm đầu ra");
        public static readonly PlcTagDefinition OutputRotateTimeout90 = CreateBit("alarm.output_rotate_timeout_90", "D5147.1", "Lỗi TimeOut Xilanh xoay 90 Cụm đầu ra");
        public static readonly PlcTagDefinition ShiftLeftTimeout = CreateBit("alarm.shift_left_timeout", "D5147.2", "Lỗi Timeout Xilanh chuyển phôi sang trái");
        public static readonly PlcTagDefinition ShiftRightTimeout = CreateBit("alarm.shift_right_timeout", "D5147.3", "Lỗi Timeout Xilanh chuyển phôi sang phải");
        public static readonly PlcTagDefinition ShiftInTimeout = CreateBit("alarm.shift_in_timeout", "D5147.4", "Lỗi Timeout Xilanh chuyển phôi ở trong");
        public static readonly PlcTagDefinition ShiftOutTimeout = CreateBit("alarm.shift_out_timeout", "D5147.5", "Lỗi Timeout Xilanh chuyển phôi ở ngoài");
        public static readonly PlcTagDefinition ProductClampTimeout = CreateBit("alarm.product_clamp_timeout", "D5147.6", "Lỗi Timeout Kẹp phôi thành phẩm");
        public static readonly PlcTagDefinition ProductUnclampTimeout = CreateBit("alarm.product_unclamp_timeout", "D5147.7", "Lỗi Timeout Mở Kẹp phôi thành phẩm");
        public static readonly PlcTagDefinition OutputMagnetInTimeout = CreateBit("alarm.output_magnet_in_timeout", "D5147.8", "Lỗi Timout Xilanh cụm Nam châm Output ở trong");
        public static readonly PlcTagDefinition OutputMagnetOutTimeout = CreateBit("alarm.output_magnet_out_timeout", "D5147.9", "Lỗi Timout Xilanh cụm Nam châm Output ở ngoài");

        // Aliases for backward compatibility
        public static readonly PlcTagDefinition HumanInWorkingZone = EStop;
        public static readonly PlcTagDefinition YLimitNegative = XLimitNegative;
        public static readonly PlcTagDefinition YLimitPositive = XLimitPositive;
        public static readonly PlcTagDefinition PickSlip = ToolCheckSensor1;
        public static readonly PlcTagDefinition PlaceSlip = ToolCheckSensor2;
        public static readonly PlcTagDefinition CanOpenDisconnect = AlarmDriverX;
        public static readonly PlcTagDefinition LostPhase = EStop;
        public static readonly PlcTagDefinition NotHomed = NotHomedRodal;
        public static readonly PlcTagDefinition AlarmDriverY = AlarmDriverX;
        public static readonly PlcTagDefinition RotateCylinderTimeout90 = RotateCylinderTimeout180;
        public static readonly PlcTagDefinition Cart1PositionInvalid = CartPositionInvalid;
        public static readonly PlcTagDefinition Cart2PositionInvalid = CartPositionInvalid;
        public static readonly PlcTagDefinition Cart1ClampCylinderFault = CartClampTimeout;
        public static readonly PlcTagDefinition Cart2ClampCylinderFault = CartClampTimeout;
        public static readonly PlcTagDefinition ToolGripperNoRelease = RotateCylinderTimeout0;
        public static readonly PlcTagDefinition ToolGripperNoClamp = RotateCylinderTimeout180;
        public static readonly PlcTagDefinition FailPlaceLine1 = ToolCheckSensor1;
        public static readonly PlcTagDefinition FailPlaceLine2 = ToolCheckSensor2;
        public static readonly PlcTagDefinition LightCurtain = EStop;
        public static readonly PlcTagDefinition YSoftLimitLeft = XSoftLimitOutside;
        public static readonly PlcTagDefinition YSoftLimitRight = XSoftLimitInside;
        public static readonly PlcTagDefinition YOverMoment = XOverMoment;
        public static readonly PlcTagDefinition PlcLine1Disconnected = CreateBit("alarm.plc_line_1_disconnected", "D5145.0", "Mất kết nối PLC line 1");
        public static readonly PlcTagDefinition PlcLine2Disconnected = CreateBit("alarm.plc_line_2_disconnected", "D5145.1", "Mất kết nối PLC line 2");
        public static readonly PlcTagDefinition PcDisconnected = CreateBit("alarm.pc_disconnected", "D5145.2", "Mất kết nối PC");
        public static readonly PlcTagDefinition OrderNotEntered = CreateBit("alarm.order_not_entered", "D5145.3", "Chưa nhập order");
        public static readonly PlcTagDefinition ProductParametersMissingLine1 = CreateBit("alarm.product_parameters_missing_line_1", "D5145.4", "Lỗi thiếu thông số sản phẩm line 1");
        public static readonly PlcTagDefinition ProductParametersMissingLine2 = CreateBit("alarm.product_parameters_missing_line_2", "D5145.5", "Lỗi thiếu thông số sản phẩm line 2");
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

    public static IReadOnlyList<PlcTagDefinition> All { get; } = CollectAllTags(
        typeof(Words),
        typeof(Inputs),
        typeof(Outputs),
        typeof(DataAutos),
        typeof(RobotTest),
        typeof(DataTrayCart),
        typeof(DataMachine),
        typeof(Manual),
        typeof(Alarms),
        typeof(Agv));

    public static IReadOnlyDictionary<string, PlcTagDefinition> ByName { get; } =
        All.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> WritableTagNames { get; } = CollectTagNames(
        typeof(Words),
        typeof(Outputs),
        typeof(DataAutos),
        typeof(RobotTest),
        typeof(DataTrayCart),
        typeof(DataMachine),
        typeof(Manual),
        typeof(Agv));

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
            .DistinctBy(static tag => tag.Name, StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }
}

