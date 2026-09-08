import {
  getUserFormValues,
  isUsernameTaken,
  normalizeUserFormValues
} from "./UserSettings";

describe("UserSettings helpers", () => {
  const users = [
    { id: 1, username: "admin" },
    { id: 2, username: "ducne" }
  ];

  it("returns blank credential fields for a new user form", () => {
    expect(getUserFormValues(null)).toMatchObject({
      username: "",
      password: "",
      role: "Operator",
      isActive: true,
      isSystemAccount: false
    });
  });

  it("detects duplicate usernames after trimming and ignores case", () => {
    expect(isUsernameTaken(users, " ADMIN ")).toBe(true);
    expect(isUsernameTaken(users, "ducne")).toBe(true);
    expect(isUsernameTaken(users, "operator1")).toBe(false);
  });

  it("allows the edited user to keep their own username", () => {
    expect(isUsernameTaken(users, "Admin", 1)).toBe(false);
    expect(isUsernameTaken(users, "Admin", 2)).toBe(true);
  });

  it("normalizes submitted user values before saving", () => {
    expect(normalizeUserFormValues({
      username: " operator1 ",
      password: "secret123",
      fullName: " Operator One ",
      email: " ",
      role: " Operator ",
      isActive: undefined,
      isSystemAccount: undefined
    }, true)).toEqual({
      username: "operator1",
      password: "secret123",
      fullName: "Operator One",
      email: null,
      role: "Operator",
      isActive: true,
      isSystemAccount: false
    });
  });
});
