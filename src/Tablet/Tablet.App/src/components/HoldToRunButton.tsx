import React, {useCallback, useRef} from 'react';
import {Pressable, StyleSheet, Text} from 'react-native';
import {colors, typography} from '../styles/theme';

interface Props {
  label: string;
  disabled?: boolean;
  active?: boolean;
  compact?: boolean;
  size?: 'standard' | 'hero';
  onStart(): Promise<void> | void;
  onStop(): Promise<void> | void;
}

export const HoldToRunButton = ({label, disabled, active, compact, size = 'standard', onStart, onStop}: Props) => {
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
        compact && styles.buttonCompact,
        size === 'hero' && styles.buttonHero,
        size === 'hero' && compact && styles.buttonHeroCompact,
        active && styles.active,
        disabled && styles.disabled,
        pressed && !disabled && styles.pressed,
      ]}>
      <Text
        style={[
          styles.text,
          compact && styles.textCompact,
          size === 'hero' && styles.textHero,
          size === 'hero' && compact && styles.textHeroCompact,
          active && styles.activeText,
          disabled && styles.disabledText,
        ]}
        numberOfLines={1}>
        {label}
      </Text>
    </Pressable>
  );
};

const styles = StyleSheet.create({
  button: {
    minHeight: 48,
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderRadius: 4,
    borderColor: '#2083C9',
    backgroundColor: '#114A7B',
    paddingHorizontal: 12,
  },
  buttonCompact: {
    minHeight: 38,
    paddingHorizontal: 8,
  },
  buttonHero: {
    minHeight: 66,
    borderColor: '#2A91D6',
    backgroundColor: '#15538A',
  },
  buttonHeroCompact: {
    minHeight: 38,
  },
  active: {
    backgroundColor: colors.runningSurface,
    borderColor: colors.running,
  },
  activeText: {
    color: colors.running,
    fontWeight: typography.weights.semibold,
  },
  disabled: {
    opacity: 0.45,
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
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.bold,
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
  textCompact: {
    fontSize: 12,
  },
  textHero: {
    fontSize: 27,
  },
  textHeroCompact: {
    fontSize: 16,
  },
});
