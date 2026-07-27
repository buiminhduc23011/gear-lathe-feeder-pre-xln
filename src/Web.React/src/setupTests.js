import "@testing-library/jest-dom";

Object.defineProperty(window, "matchMedia", {
  writable: true,
  value: jest.fn().mockImplementation((query) => {
    const mediaQueryList = {
      matches: false,
      media: query,
      onchange: null,
      addListener: jest.fn((listener) => listener(mediaQueryList)),
      removeListener: jest.fn(),
      addEventListener: jest.fn((eventName, listener) => {
        if (eventName === "change") {
          listener(mediaQueryList);
        }
      }),
      removeEventListener: jest.fn(),
      dispatchEvent: jest.fn()
    };

    return mediaQueryList;
  })
});

Object.defineProperty(window.URL, "createObjectURL", {
  writable: true,
  value: jest.fn(() => "blob:mock-url")
});

Object.defineProperty(window.URL, "revokeObjectURL", {
  writable: true,
  value: jest.fn()
});
