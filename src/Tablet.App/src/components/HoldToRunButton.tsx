import React, {useCallback, useRef} from 'react';
import {Pressable, StyleSheet, Text} from 'react-native';
import {colors, radius} from '../styles/theme';

interface Props {
  label: string;
  disabled?: boolean;
  active?: boolean;
  onStart(): Promise<void> | void;
  onStop(): Promise<void> | void;
}

export const HoldToRunButton = ({label, disabled, active, onStart, onStop}: Props) => {
  const pressedRef = useRef(false);

  const start = useCallback(() => {
    if (disabled || pressedRef.current) {
      return;
    }
    pressedRef.current = true;
    void onStart();
  }, [disabled, onStart]);

  const stop = useCallback(() => {
    if (!pressedRef.current) {
      return;
    }
    pressedRef.current = false;
    void onStop();
  }, [onStop]);

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      disabled={disabled}
      onPressIn={start}
      onPressOut={stop}
      onResponderTerminate={stop}
      style={({pressed}) => [
        styles.button,
        active && styles.active,
        disabled && styles.disabled,
        pressed && !disabled && styles.pressed,
      ]}>
      <Text style={[styles.text, active && styles.activeText, disabled && styles.disabledText]} numberOfLines={1}>
        {label}
      </Text>
    </Pressable>
  );
};

const styles = StyleSheet.create({
  button: {
    minHeight: 56,
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderRadius: radius.button,
    borderColor: colors.borderStrong,
    backgroundColor: colors.raised,
    paddingHorizontal: 12,
  },
  active: {
    backgroundColor: colors.runningSurface,
    borderColor: colors.running,
  },
  activeText: {
    color: colors.running,
    fontWeight: '600',
  },
  disabled: {
    opacity: 0.3,
  },
  disabledText: {
    color: colors.textDisabled,
  },
  pressed: {
    backgroundColor: colors.primarySurface,
    borderColor: colors.primaryStrong,
  },
  text: {
    color: colors.text,
    fontSize: 14,
    fontWeight: '600',
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
});
