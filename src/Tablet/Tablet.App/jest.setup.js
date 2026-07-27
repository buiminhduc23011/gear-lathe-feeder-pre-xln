jest.mock('react-native-tcp-socket', () => ({
  createConnection: jest.fn(),
}));

const createAntdMock = () => {
  const React = require('react');
  const {Modal: RNModal, Pressable} = require('react-native');

  return {
    Provider: ({children}) => children,
    Button: ({children, activeStyle: _activeStyle, style, ...props}) =>
      React.createElement(Pressable, {...props, style}, children),
    Modal: ({children, footer: _footer, onClose, visible, ...props}) =>
      React.createElement(RNModal, {visible, transparent: true, ...props, onRequestClose: onClose}, children),
  };
};

jest.mock('@ant-design/react-native', () => createAntdMock());
jest.mock('@ant-design/react-native/lib/button', () => createAntdMock().Button);
jest.mock('@ant-design/react-native/lib/modal', () => createAntdMock().Modal);
jest.mock('@ant-design/react-native/lib/provider', () => createAntdMock().Provider);
