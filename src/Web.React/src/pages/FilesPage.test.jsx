import { render, screen, waitFor } from "@testing-library/react";
import FilesPage from "./FilesPage";

jest.mock("antd", () => {
  const actual = jest.requireActual("antd");

  return {
    ...actual,
    Table: ({ columns, dataSource, rowKey }) => (
      <table data-testid="files-table">
        <tbody>
          {dataSource.map((record, rowIndex) => (
            <tr key={record[rowKey] ?? rowIndex}>
              {columns.map((column, columnIndex) => {
                const rawValue = column.dataIndex ? record[column.dataIndex] : undefined;
                const content = column.render ? column.render(rawValue, record, rowIndex) : rawValue;

                return <td key={column.key ?? column.dataIndex ?? columnIndex}>{content}</td>;
              })}
            </tr>
          ))}
        </tbody>
      </table>
    )
  };
});

jest.mock("../contexts/AuthContext", () => ({
  useAuth: jest.fn()
}));

jest.mock("../config/api", () => ({
  API_ENDPOINTS: {
    files: "/api/files",
    machines: "/api/machines"
  },
  apiClient: {
    get: jest.fn(),
    post: jest.fn(),
    delete: jest.fn()
  },
  getApiErrorMessage: jest.fn(() => "Lỗi")
}));

const { useAuth } = require("../contexts/AuthContext");
const { apiClient } = require("../config/api");

describe("FilesPage", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("shows upload action for admin and renders file rows", async () => {
    useAuth.mockReturnValue({
      currentUser: { role: "Admin", fullName: "Admin User" }
    });

    apiClient.get
      .mockResolvedValueOnce({
        data: [
          {
            id: 1,
            originalFileName: "report.txt",
            storedFileName: "report_20260323120000000.txt",
            size: 1024,
            machineName: "Pinion Grinding Robot 01",
            manufacturer: "STI",
            uploadedAtUtc: "2026-03-23T12:00:00Z",
            sentAtUtc: "2026-03-23T11:59:00Z",
            uploadSource: "DeviceApi",
            canDelete: true
          }
        ]
      })
      .mockResolvedValueOnce({
        data: [
          {
            machineId: 1,
            machineCode: "PGR-01",
            machineName: "Pinion Grinding Robot 01",
            manufacturer: "STI"
          }
        ]
      });

    render(<FilesPage />);

    expect(await screen.findByText("report.txt")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Upload tay/i })).toBeInTheDocument();
    expect(screen.getByText("Pinion Grinding Robot 01")).toBeInTheDocument();
    expect(screen.getAllByText("1 KB").length).toBeGreaterThan(0);
  });

  it("hides upload action for viewer", async () => {
    useAuth.mockReturnValue({
      currentUser: { role: "Viewer", fullName: "Viewer User" }
    });

    apiClient.get.mockResolvedValueOnce({
      data: []
    });

    render(<FilesPage />);

    await waitFor(() => expect(apiClient.get).toHaveBeenCalledWith("/api/files"));
    expect(screen.queryByRole("button", { name: /Upload tay/i })).not.toBeInTheDocument();
  });
});
