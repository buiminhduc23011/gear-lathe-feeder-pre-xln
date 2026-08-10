import React from 'react';
import AntButton from '@ant-design/react-native/lib/button';
import AntModal from '@ant-design/react-native/lib/modal';
import AntProvider from '@ant-design/react-native/lib/provider';
import type {GestureResponderEvent, StyleProp, ViewStyle} from 'react-native';

interface ProviderProps {
  children: React.ReactNode;
}

interface ButtonProps {
  accessibilityLabel?: string;
  accessibilityRole?: 'button';
  activeStyle?: StyleProp<ViewStyle> | false;
  children: React.ReactNode;
  disabled?: boolean;
  onPress?: (event: GestureResponderEvent) => void;
  style?: StyleProp<ViewStyle>;
}

interface ModalProps {
  animationType?: 'none' | 'fade' | 'slide-up' | 'slide-down' | 'slide';
  bodyStyle?: StyleProp<ViewStyle>;
  children: React.ReactNode;
  maskClosable?: boolean;
  onClose(): void;
  onRequestClose?(): boolean;
  style?: StyleProp<ViewStyle>;
  transparent?: boolean;
  visible: boolean;
}

export const AppProvider = ({children}: ProviderProps) => <AntProvider>{children}</AntProvider>;

export const HmiButton = ({children, style, activeStyle, ...props}: ButtonProps) => (
  <AntButton {...props} activeStyle={activeStyle as any} style={style as any}>
    {children}
  </AntButton>
);

export const HmiModal = ({children, ...props}: ModalProps) => (
  <AntModal {...props} footer={[]}>
    {children}
  </AntModal>
);
