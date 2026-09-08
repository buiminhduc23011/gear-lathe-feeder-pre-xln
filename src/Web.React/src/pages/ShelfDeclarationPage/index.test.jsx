import React from "react";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import ShelfDeclarationPage from "./index";

jest.setTimeout(20000);

jest.mock("antd", () => {
  const ReactLib = require("react");
  const actual = jest.requireActual("antd");

  const MockSelect = ({ options = [], value, onChange, placeholder, allowClear, style, disabled, ...rest }) => {
    const dataAttributes = Object.fromEntries(
      Object.entries(rest).filter(([key]) => key.startsWith("data-") || key.startsWith("aria-"))
    );

    return (
    <select
      aria-label={placeholder || rest["aria-label"] || "select"}
      value={value ?? ""}
      onChange={(event) => {
        const nextOption = options.find((option) => String(option.value) === event.target.value);
        onChange?.(nextOption ? nextOption.value : undefined);
      }}
      style={style}
      disabled={disabled}
      {...dataAttributes}
    >
      {allowClear || placeholder ? <option value="">{placeholder || "clear"}</option> : null}
      {options.map((option) => (
        <option key={String(option.value)} value={String(option.value)} disabled={Boolean(option.disabled)}>
          {option.label}
        </option>
      ))}
    </select>
    );
  };

  const MockInputNumber = ({ value, onChange, onKeyDown, min, max, placeholder, disabled, status, ...rest }) => {
    const dataAttributes = Object.fromEntries(
      Object.entries(rest).filter(([key]) => key.startsWith("data-") || key.startsWith("aria-"))
    );

    return (
    <input
      type="text"
      value={value ?? ""}
      min={min}
      max={max}
      placeholder={placeholder}
      disabled={disabled}
      aria-invalid={status === "error" ? "true" : undefined}
      {...dataAttributes}
      onChange={(event) => {
        if (event.target.value === "") {
          onChange?.(null);
          return;
        }

        const next = Number(event.target.value);
        if (Number.isFinite(next)) {
          onChange?.(next);
        }
      }}
      onKeyDown={onKeyDown}
    />
    );
  };

  const MockTable = ({ columns, dataSource, rowKey, locale, onRow }) => (
    <table>
      <tbody>
        {dataSource.length === 0 ? (
          <tr>
            <td>{locale?.emptyText ?? null}</td>
          </tr>
        ) : dataSource.map((record, rowIndex) => (
          <tr key={record[rowKey] ?? rowIndex} {...(onRow?.(record, rowIndex) ?? {})}>
            {columns.map((column, columnIndex) => {
              const rawValue = column.dataIndex ? record[column.dataIndex] : undefined;
              const content = column.render ? column.render(rawValue, record, rowIndex) : rawValue;
              return <td key={column.key ?? column.dataIndex ?? columnIndex}>{content}</td>;
            })}
          </tr>
        ))}
      </tbody>
    </table>
  );

  const MockTabs = ({ items, defaultActiveKey }) => {
    const [activeKey, setActiveKey] = ReactLib.useState(defaultActiveKey || items[0]?.key);
    const activeItem = items.find((item) => item.key === activeKey) ?? items[0];

    return (
      <div>
        <div>
          {items.map((item) => (
            <button key={item.key} type="button" onClick={() => setActiveKey(item.key)}>
              {item.label}
            </button>
          ))}
        </div>
        <div>{activeItem?.children}</div>
      </div>
    );
  };

  return {
    ...actual,
    Form: Object.assign(
      ({ children }) => <form>{children}</form>,
      {
        Item: ({ label, children }) => (
          <div>
            {label ? <div>{label}</div> : null}
            {children}
          </div>
        ),
        useForm: () => [{}]
      }
    ),
    Row: ({ children }) => <div>{children}</div>,
    Col: ({ children }) => <div>{children}</div>,
    Select: MockSelect,
    InputNumber: MockInputNumber,
    Table: MockTable,
    Tabs: MockTabs,
    Tooltip: ({ children }) => <>{children}</>,
    message: {
      warning: jest.fn(),
      success: jest.fn(),
      error: jest.fn()
    }
  };
});

jest.mock("../../config/api", () => ({
  API_ENDPOINTS: {
    machines: "/api/machines",
    machineModels: (machineId) => `/api/machines/${machineId}/models`,
    shelfDeclarations: (machineId) => `/api/machines/${machineId}/shelf-declarations`,
    shelfDeclarationSlotStatus: (machineId) => `/api/machines/${machineId}/shelf-declaration-slot-status`,
    agvCallEligibility: (machineCode, machineSlotIndex) => `/api/shelf-declarations/agv-call-eligibility?machineCode=${machineCode}&machineSlotIndex=${machineSlotIndex}`
  },
  apiClient: {
    get: jest.fn(),
    post: jest.fn(),
    delete: jest.fn()
  },
  getApiErrorMessage: jest.fn(() => "Loi")
}));

const { apiClient } = require("../../config/api");
const { message } = require("antd");

describe("ShelfDeclarationPage", () => {
  let includeActiveManualDeclaration = false;

  beforeAll(() => {
    Object.defineProperty(window, "matchMedia", {
      writable: true,
      value: jest.fn().mockImplementation((query) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: jest.fn(),
        removeListener: jest.fn(),
        addEventListener: jest.fn(),
        removeEventListener: jest.fn(),
        dispatchEvent: jest.fn()
      }))
    });
  });

  beforeEach(() => {
    jest.clearAllMocks();
    includeActiveManualDeclaration = false;

    apiClient.get.mockImplementation((url) => {
      switch (url) {
        case "/api/machines":
          return Promise.resolve({
            data: [
              {
                machineId: 1,
                machineCode: "PGR-01",
                machineName: "Gear Lathe Feeder Robot 01",
                stagingSlotIndices: [1],
                jig1HeightMm: 150,
                jig2HeightMm: 200,
                jig3HeightMm: 120,
                jig4HeightMm: 100
              }
            ]
          });
        case "/api/machines/1/models":
          return Promise.resolve({
            data: [
              { modelName: "Model A", trayType: 0, orderInput: 1, isDeleted: false, robotData: { jigSupplyType: 1, inputBlankThickness: 10 } },
              { modelName: "Model B", trayType: 1, orderInput: 0, isDeleted: false, robotData: { jigSupplyType: 2, inputBlankThickness: 10 } },
              { modelName: "Model C", trayType: 1, orderInput: 1, isDeleted: false, isEnabled: false, robotData: { jigSupplyType: 3, inputBlankThickness: 10 } }
            ]
          });
        case "/api/machines/1/shelf-declarations":
          return Promise.resolve({
            data: [
              {
                id: 10,
                mode: "Agv",
                status: "Created",
                stagingSlotIndex: 1,
                shelfLayoutType: 0,
                shelfLayoutName: "Xe hang 4 vi tri",
                orderCount: 2,
                ordersJson: JSON.stringify([
                  {
                    orderId: "ORD-BUSY-01",
                    modelName: "Model Busy A",
                    quantity: 2,
                    cartPositionIndex: 1,
                    orderSequence: 1,
                    jigType: 1
                  },
                  {
                    orderId: "ORD-BUSY-02",
                    modelName: "Model Busy B",
                    quantity: 3,
                    cartPositionIndex: 2,
                    orderSequence: 2,
                    jigType: 2
                  }
                ]),
                createdByUsername: "operator"
              },
              {
                id: 11,
                mode: "ManualLoad",
                status: includeActiveManualDeclaration ? "Created" : "Completed",
                machineSlotIndex: 1,
                shelfLayoutType: 0,
                shelfLayoutName: "Xe hang 4 vi tri",
                orderCount: 2,
                ordersJson: JSON.stringify([
                  {
                    orderId: "MANUAL-BUSY-01",
                    modelName: "Manual Busy A",
                    reportModelName: "Manual Busy A",
                    quantity: 4,
                    cartPositionIndex: 1,
                    orderSequence: 1,
                    jigType: 1
                  },
                  {
                    orderId: "MANUAL-BUSY-02",
                    modelName: "Manual Busy B",
                    reportModelName: "Manual Busy B",
                    quantity: 3,
                    cartPositionIndex: 2,
                    orderSequence: 2,
                    jigType: 2
                  }
                ]),
                createdByUsername: "operator"
              },
              {
                id: 13,
                mode: "Agv",
                status: "Created",
                stagingSlotIndex: 2,
                shelfLayoutName: "Xe hang 4 vi tri",
                orderCount: 1,
                createdByUsername: "operator",
                pickedByAgvId: "AGV-01",
                pickedByAgvName: "AGV 01",
                agvTakenAtUtc: "2026-04-02T04:30:00Z"
              },
              {
                id: 12,
                mode: "Agv",
                status: "Completed",
                stagingSlotIndex: 2,
                shelfLayoutName: "Xe hang 4 vi tri",
                orderCount: 1,
                createdByUsername: "operator"
              }
            ]
          });
        case "/api/machines/1/shelf-declaration-slot-status":
          return Promise.resolve({
            data: [
              { slotIndex: 1, isOccupied: true, shelfLayoutName: "Xe hang 4 vi tri", orderCount: 2 }
            ]
          });
        case "/api/shelf-declarations/agv-call-eligibility?machineCode=PGR-01&machineSlotIndex=1":
          return Promise.resolve({
            data: {
              machineId: 1,
              machineCode: "PGR-01",
              machineName: "Gear Lathe Feeder Robot 01",
              machineSlotIndex: 1,
              stagingSlotIndex: 1,
              hasActiveDeclaration: true,
              declarationId: 10,
              declarationStatus: "Created",
              orderCount: 2,
              reasonCode: "Eligible",
              reasonMessage: "Eligible for AGV call."
            }
          });
        case "/api/shelf-declarations/agv-call-eligibility?machineCode=PGR-01&machineSlotIndex=2":
          return Promise.resolve({
            data: {
              machineId: 1,
              machineCode: "PGR-01",
              machineName: "Gear Lathe Feeder Robot 01",
              machineSlotIndex: 2,
              stagingSlotIndex: 2,
              hasActiveDeclaration: false,
              declarationId: null,
              declarationStatus: null,
              orderCount: null,
              reasonCode: "NoDeclaration",
              reasonMessage: "No active AGV declaration found for machine slot 2 (staging slot 2)."
            }
          });
        default:
          return Promise.resolve({ data: [] });
      }
    });

    apiClient.post.mockResolvedValue({ data: {} });
    apiClient.delete.mockResolvedValue({ data: {} });
  });

  it("renders create flow and removes manual request action from history", async () => {
    render(<ShelfDeclarationPage />);

    expect((await screen.findAllByRole("heading", { name: /Khai|create/i })).length).toBeGreaterThan(0);
    expect((await screen.findAllByText("Gear Lathe Feeder Robot 01 (PGR-01)")).length).toBeGreaterThan(0);

    await waitFor(() => {
      expect(screen.getByTestId("staging-slot-1")).toHaveAttribute("aria-disabled", "false");
      expect(screen.getByTestId("staging-slot-1")).toHaveAttribute("aria-busy", "true");
      expect(screen.getByTestId("staging-slot-2")).toHaveAttribute("aria-disabled", "true");
      expect(screen.getByTestId("staging-slot-3")).toHaveAttribute("aria-disabled", "true");
      expect(screen.getByTestId("staging-slot-4")).toHaveAttribute("aria-disabled", "true");
    });

    fireEvent.click(screen.getByTestId("staging-slot-1"));
    expect(screen.getByTestId("staging-slot-1")).toHaveAttribute("aria-pressed", "true");
    expect(screen.queryByText(/Canh bao goi AGV/i)).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: /send/i })).toBeDisabled();

    fireEvent.click(screen.getByRole("radio", { name: /Th/i }));
    expect(await screen.findByText("Machine slot 1")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Thêm order|plus/i }));
    fireEvent.change(screen.getByPlaceholderText("Nhập Article ID"), { target: { value: "Model A" } });
    expect(await screen.findByText("Model A")).toBeInTheDocument();
    expect(await screen.findByText(/Trực quan hóa .* Xe hàng/i)).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Lịch sử khai báo|Lich su khai bao/i }));
    expect(await screen.findByRole("heading", { name: /Lịch sử khai báo|Lich su khai bao/i })).toBeInTheDocument();
    expect(screen.queryByText(/Thu cong|Thá»§ cÃ´ng/i)).not.toBeInTheDocument();
    expect(screen.getAllByRole("button", { name: /Hủy|Huy/i })).toHaveLength(1);

    await waitFor(() => {
      expect(apiClient.get).toHaveBeenCalledWith("/api/machines");
      expect(apiClient.get).toHaveBeenCalledWith("/api/machines/1/models");
      expect(apiClient.get).toHaveBeenCalledWith("/api/machines/1/shelf-declarations");
      expect(apiClient.get).toHaveBeenCalledWith("/api/machines/1/shelf-declaration-slot-status");
    });
  });

  it("auto-generates and locks Order when the resolved model disables order input", async () => {
    render(<ShelfDeclarationPage />);

    expect((await screen.findAllByText("Gear Lathe Feeder Robot 01 (PGR-01)")).length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole("radio", { name: /Th/i }));
    expect(await screen.findByText("Machine slot 1")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Thêm order|plus/i }));
    fireEvent.change(screen.getByPlaceholderText("Nhập Article ID"), { target: { value: "Model B" } });

    const generatedOrder = await screen.findByDisplayValue(/^MODELB-\d{12}-01$/);
    expect(generatedOrder).toBeDisabled();
    fireEvent.change(screen.getByPlaceholderText("Nhập SL"), { target: { value: "1" } });

    fireEvent.click(screen.getByRole("button", { name: /send/i }));

    await waitFor(() => {
      expect(apiClient.post).toHaveBeenCalled();
    });

    const [, payload] = apiClient.post.mock.calls[0];
    expect(payload.orders[0]).toMatchObject({
      modelName: "Model B",
      reportModelName: "Model B",
      quantity: 1
    });
    expect(payload.orders[0].orderId).toMatch(/^MODELB-\d{12}-01$/);
  });

  it("allows selecting an active manual slot to preview its declaration", async () => {
    includeActiveManualDeclaration = true;
    render(<ShelfDeclarationPage />);

    expect((await screen.findAllByText("Gear Lathe Feeder Robot 01 (PGR-01)")).length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole("radio", { name: /Th/i }));
    expect(await screen.findByText("Machine slot 1")).toBeInTheDocument();

    const busySlot = screen.getByTestId("machine-slot-1");
    expect(busySlot).not.toHaveAttribute("aria-disabled", "true");
    fireEvent.click(busySlot);

    expect(await screen.findByText("Machine slot 1")).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.getAllByText("Manual Busy A").length).toBeGreaterThan(0);
    });
    expect(screen.getAllByText("Manual Busy B").length).toBeGreaterThan(0);
    expect(screen.getByText(/Trực quan hóa .* Xe hàng/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /send/i })).toBeDisabled();
    expect(screen.getByRole("button", { name: /ThÃªm order|plus/i })).toBeDisabled();
  });

  it("clears auto-generated Order when switching to a model that requires order input", async () => {
    render(<ShelfDeclarationPage />);

    expect((await screen.findAllByText("Gear Lathe Feeder Robot 01 (PGR-01)")).length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole("radio", { name: /Th/i }));
    expect(await screen.findByText("Machine slot 1")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Thêm order|plus/i }));
    const modelOrderInput = screen.getByPlaceholderText("Nhập Article ID");
    fireEvent.change(modelOrderInput, { target: { value: "Model B" } });
    expect(await screen.findByDisplayValue(/^MODELB-\d{12}-01$/)).toBeDisabled();

    fireEvent.change(modelOrderInput, { target: { value: "Model A" } });

    const manualOrderInput = await screen.findByPlaceholderText("Nhập Order");
    expect(manualOrderInput).not.toBeDisabled();
    expect(manualOrderInput).toHaveValue("");
  });

  it("allocates an overflow order and moves a different jig to the next cart position", async () => {
    render(<ShelfDeclarationPage />);

    expect((await screen.findAllByText("Gear Lathe Feeder Robot 01 (PGR-01)")).length).toBeGreaterThan(0);
    fireEvent.click(screen.getByRole("radio", { name: /Th/i }));
    expect(await screen.findByText("Machine slot 1")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Thêm order|plus/i }));
    fireEvent.change(screen.getByPlaceholderText("Nhập Article ID"), { target: { value: "Model A" } });
    fireEvent.change(screen.getByPlaceholderText("Nhập Order"), { target: { value: "ORD-A-01" } });
    fireEvent.change(screen.getByPlaceholderText("Nhập SL"), { target: { value: "20" } });

    expect(await screen.findByText(/Vị trí 1: 15/)).toBeInTheDocument();
    expect(await screen.findByText(/Vị trí 2: 5/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Thêm order|plus/i }));
    fireEvent.change(screen.getAllByPlaceholderText("Nhập Article ID")[1], { target: { value: "Model B" } });
    await screen.findByDisplayValue(/^MODELB-\d{12}-02$/);
    fireEvent.change(screen.getAllByPlaceholderText("Nhập SL")[1], { target: { value: "1" } });

    expect(await screen.findByText(/Vị trí 3: 1/)).toBeInTheDocument();
  });

  it("allows inactive models in the order declaration flow", async () => {
    render(<ShelfDeclarationPage />);

    expect((await screen.findAllByText("Gear Lathe Feeder Robot 01 (PGR-01)")).length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole("radio", { name: /Th/i }));
    expect(await screen.findByText("Machine slot 1")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Thêm order|plus/i }));
    const articleOrderInput = screen.getByPlaceholderText("Nhập Article ID");
    fireEvent.change(articleOrderInput, { target: { value: "Model C" } });
    fireEvent.keyDown(articleOrderInput, { key: "Enter" });

    expect(await screen.findByText("Model C")).toBeInTheDocument();
    fireEvent.change(screen.getByPlaceholderText(/Order/), { target: { value: "ORD-INACTIVE-01" } });
    fireEvent.change(screen.getByPlaceholderText(/SL/), { target: { value: "1" } });
    fireEvent.click(screen.getByRole("button", { name: /send/i }));

    await waitFor(() => {
      expect(apiClient.post).toHaveBeenCalled();
    });

    const [, payload] = apiClient.post.mock.calls[0];
    expect(payload.orders[0]).toMatchObject({
      modelName: "Model C",
      reportModelName: "Model C",
      orderId: "ORD-INACTIVE-01",
      quantity: 1
    });
  });

  it("does not advance from quantity when the typed value is not numeric", async () => {
    render(<ShelfDeclarationPage />);

    expect((await screen.findAllByText("Gear Lathe Feeder Robot 01 (PGR-01)")).length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole("radio", { name: /Th/i }));
    expect(await screen.findByText("Machine slot 1")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Thêm order|plus/i }));
    fireEvent.change(screen.getByPlaceholderText("Nhập Article ID"), { target: { value: "Model B" } });

    await screen.findByDisplayValue(/^MODELB-\d{12}-01$/);
    const quantityInput = screen.getByPlaceholderText("Nhập SL");
    fireEvent.change(quantityInput, { target: { value: "adss" } });
    fireEvent.keyDown(quantityInput, { key: "Enter" });

    expect(quantityInput).toHaveAttribute("aria-invalid", "true");
    expect(screen.getAllByPlaceholderText("Nhập Article ID")).toHaveLength(1);
  });
});
