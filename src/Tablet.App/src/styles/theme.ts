export const colors = {
  window: '#04111F',
  shell: '#071524',
  panel: '#0C1B2D',
  subpanel: '#10263D',
  recessed: '#04101C',
  raised: '#0F2235',
  glass: 'rgba(255, 255, 255, 0.045)',
  border: '#173454',
  borderStrong: '#2B5B89',
  divider: '#13283F',
  text: '#F5F7FA',
  textSecondary: '#B5C5DE',
  textMuted: '#7F92AF',
  textDisabled: '#53657B',
  inverse: '#04101B',
  primary: '#12A4FF',
  primaryStrong: '#45D9FF',
  primarySurface: 'rgba(18, 164, 255, 0.16)',
  running: '#17F081',
  runningSurface: 'rgba(23, 240, 129, 0.18)',
  warning: '#FF950D',
  warningSurface: 'rgba(255, 149, 13, 0.18)',
  fault: '#FF4D5E',
  faultSurface: 'rgba(255, 77, 94, 0.16)',
  idle: '#9C62FF',
  idleSurface: 'rgba(156, 98, 255, 0.17)',
  cyan: '#20E8F4',
  cyanSurface: 'rgba(32, 232, 244, 0.16)',
};

export const spacing = {
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 24,
  xxl: 32,
};

export const radius = {
  button: 6,
  panel: 8,
  card: 8,
  badge: 6,
};

export const typography = {
  fontFamily: 'sans-serif',
  weights: {
    regular: '400',
    medium: '500',
    semibold: '600',
    bold: '700',
  },
} as const;
