import { render, screen } from "@testing-library/react";
import { AuthContext } from "./contexts/AuthContext";
import SettingsPage from "./pages/SettingsPage";

jest.mock("./pages/SettingsPage/MachineSettings", () => function MockMachineSettings() {
  return <div>Machine Settings Content</div>;
});

jest.mock("./pages/SettingsPage/UserSettings", () => function MockUserSettings() {
  return <div>User Settings Content</div>;
});

describe("SettingsPage", () => {
  it("shows machine and user tabs for admin", () => {
    render(
      <AuthContext.Provider value={{ currentUser: { role: "Admin" } }}>
        <SettingsPage />
      </AuthContext.Provider>
    );

    expect(screen.getByText("Machines")).toBeInTheDocument();
    expect(screen.getByText("Users")).toBeInTheDocument();
  });

  it("hides users tab for technician", () => {
    render(
      <AuthContext.Provider value={{ currentUser: { role: "Technician" } }}>
        <SettingsPage />
      </AuthContext.Provider>
    );

    expect(screen.getByText("Machines")).toBeInTheDocument();
    expect(screen.queryByText("Users")).not.toBeInTheDocument();
  });
});
