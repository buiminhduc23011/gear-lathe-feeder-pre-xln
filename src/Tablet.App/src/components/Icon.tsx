import React from 'react';
import { StyleSheet, View } from 'react-native';

export type IconName =
  | 'home'
  | 'menu'
  | 'apps'
  | 'auto'
  | 'manual'
  | 'io'
  | 'history'
  | 'settings'
  | 'plus'
  | 'delete'
  | 'save'
  | 'play'
  | 'refresh'
  | 'chevron-down'
  | 'chevron-right';

interface Props {
  name: IconName;
  color: string;
  size?: number;
}

export const Icon = ({ name, color, size = 24 }: Props) => {
  const half = size / 2;
  const stroke = Math.max(1.5, size / 12);

  switch (name) {
    case 'home':
      // House shape: Google Material / AntDesign style
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          <View style={[styles.homeRoof, { borderBottomColor: color, borderBottomWidth: half, borderLeftWidth: half, borderRightWidth: half }]} />
          <View style={[styles.homeBody, { borderColor: color, borderWidth: stroke, width: size * 0.8, height: half, top: -stroke }]} />
        </View>
      );

    case 'menu':
      // Google Material Menu (hamburger) icon
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center', gap: Math.max(3.5, size / 6) }}>
          <View style={{ backgroundColor: color, width: size * 0.75, height: stroke * 1.2, borderRadius: stroke / 2 }} />
          <View style={{ backgroundColor: color, width: size * 0.75, height: stroke * 1.2, borderRadius: stroke / 2 }} />
          <View style={{ backgroundColor: color, width: size * 0.75, height: stroke * 1.2, borderRadius: stroke / 2 }} />
        </View>
      );

    case 'apps':
      // Google Material Apps (grid) icon
      const boxSize = size * 0.3;
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          <View style={{ flexDirection: 'row', gap: stroke * 1.5, marginBottom: stroke * 1.5 }}>
            <View style={{ borderColor: color, borderWidth: stroke * 1.2, width: boxSize, height: boxSize, borderRadius: stroke * 0.5 }} />
            <View style={{ borderColor: color, borderWidth: stroke * 1.2, width: boxSize, height: boxSize, borderRadius: stroke * 0.5 }} />
          </View>
          <View style={{ flexDirection: 'row', gap: stroke * 1.5 }}>
            <View style={{ borderColor: color, borderWidth: stroke * 1.2, width: boxSize, height: boxSize, borderRadius: stroke * 0.5 }} />
            <View style={{ borderColor: color, borderWidth: stroke * 1.2, width: boxSize, height: boxSize, borderRadius: stroke * 0.5 }} />
          </View>
        </View>
      );

    case 'auto':
      // Cycle ring with a directional arrow.
      return (
        <View style={[styles.iconCanvas, { width: size, height: size }]}>
          <View style={[styles.circleOutline, { borderColor: color, borderWidth: stroke, width: size * 0.74, height: size * 0.74, borderRadius: size * 0.37 }]} />
          <View style={[styles.autoArrowHead, { borderLeftColor: color, borderLeftWidth: stroke * 3, borderTopWidth: stroke * 2.2, borderBottomWidth: stroke * 2.2, right: size * 0.04, top: size * 0.17, transform: [{ rotate: '-20deg' }] }]} />
          <View style={[styles.autoDot, { backgroundColor: color, width: stroke * 2.8, height: stroke * 2.8, borderRadius: stroke * 1.4, left: size * 0.24, bottom: size * 0.12 }]} />
        </View>
      );

    case 'manual':
      // Solid mechanic wrench (industrial manual/service mode standard)
      return (
        <View style={[styles.iconCanvas, { width: size, height: size }]}>
          {/* Handle */}
          <View style={{
            backgroundColor: color,
            width: stroke * 1.8,
            height: size * 0.6,
            borderRadius: stroke,
            transform: [{ rotate: '-45deg' }],
            position: 'absolute'
          }} />
          {/* Head */}
          <View style={{
            borderColor: color,
            borderWidth: stroke * 1.5,
            width: size * 0.44,
            height: size * 0.44,
            borderRadius: size * 0.22,
            position: 'absolute',
            top: size * 0.1,
            right: size * 0.1,
            justifyContent: 'center',
            alignItems: 'center',
            backgroundColor: 'transparent'
          }}>
            {/* Cutout */}
            <View style={{
              backgroundColor: '#090C15', // matching fallback background
              width: size * 0.2,
              height: size * 0.2,
              transform: [{ rotate: '45deg' }]
            }} />
          </View>
          {/* Bottom end ring */}
          <View style={{
            borderColor: color,
            borderWidth: stroke,
            width: size * 0.22,
            height: size * 0.22,
            borderRadius: size * 0.11,
            position: 'absolute',
            bottom: size * 0.1,
            left: size * 0.1
          }} />
        </View>
      );

    case 'io':
      // Industrial Terminal block with Input channels (left) and Output channels (right)
      return (
        <View style={[styles.iconCanvas, { width: size, height: size }]}>
          <View style={{
            borderColor: color,
            borderWidth: stroke,
            borderRadius: size * 0.1,
            width: size * 0.9,
            height: size * 0.9,
            position: 'relative',
            justifyContent: 'center',
            alignItems: 'center'
          }}>
            {/* Middle line */}
            <View style={{
              borderLeftColor: color,
              borderLeftWidth: stroke,
              borderStyle: 'dashed',
              height: '100%',
              position: 'absolute',
              left: '50%'
            }} />
            {/* Inputs (left side ports) */}
            <View style={{ position: 'absolute', left: size * 0.12, top: size * 0.12, width: stroke * 2.2, height: stroke * 2.2, borderRadius: stroke * 1.1, backgroundColor: color }} />
            <View style={{ position: 'absolute', left: size * 0.12, top: size * 0.38, width: stroke * 2.2, height: stroke * 2.2, borderRadius: stroke * 1.1, backgroundColor: color }} />
            <View style={{ position: 'absolute', left: size * 0.12, top: size * 0.64, width: stroke * 2.2, height: stroke * 2.2, borderRadius: stroke * 1.1, backgroundColor: color }} />
            {/* Outputs (right side ports) */}
            <View style={{ position: 'absolute', right: size * 0.12, top: size * 0.12, width: stroke * 2.2, height: stroke * 2.2, borderRadius: stroke * 1.1, backgroundColor: color }} />
            <View style={{ position: 'absolute', right: size * 0.12, top: size * 0.38, width: stroke * 2.2, height: stroke * 2.2, borderRadius: stroke * 1.1, backgroundColor: color }} />
            <View style={{ position: 'absolute', right: size * 0.12, top: size * 0.64, width: stroke * 2.2, height: stroke * 2.2, borderRadius: stroke * 1.1, backgroundColor: color }} />
          </View>
        </View>
      );

    case 'history':
      // History event logs clock with anticlockwise retro curve
      return (
        <View style={[styles.iconCanvas, { width: size, height: size }]}>
          <View style={[styles.circleOutline, { borderColor: color, borderWidth: stroke, width: size * 0.9, height: size * 0.9, borderRadius: size * 0.45 }]}>
            <View style={[styles.clockHandHour, { backgroundColor: color, width: stroke, height: half * 0.5, bottom: half * 0.45, left: half * 0.45, transform: [{ translateY: -half * 0.25 }] }]} />
            <View style={[styles.clockHandMinute, { backgroundColor: color, height: stroke, width: half * 0.6, bottom: half * 0.45, left: half * 0.45, transform: [{ translateX: half * 0.3 }] }]} />
          </View>
        </View>
      );

    case 'settings':
      // Radial settings gear.
      return (
        <View style={[styles.iconCanvas, { width: size, height: size }]}>
          <View style={[styles.gearTooth, { backgroundColor: color, width: stroke * 1.8, height: size * 0.18, borderRadius: stroke, top: size * 0.02 }]} />
          <View style={[styles.gearTooth, { backgroundColor: color, width: stroke * 1.8, height: size * 0.18, borderRadius: stroke, bottom: size * 0.02 }]} />
          <View style={[styles.gearTooth, { backgroundColor: color, width: size * 0.18, height: stroke * 1.8, borderRadius: stroke, left: size * 0.02 }]} />
          <View style={[styles.gearTooth, { backgroundColor: color, width: size * 0.18, height: stroke * 1.8, borderRadius: stroke, right: size * 0.02 }]} />
          <View style={[styles.gearTooth, { backgroundColor: color, width: stroke * 1.7, height: size * 0.17, borderRadius: stroke, right: size * 0.13, top: size * 0.13, transform: [{ rotate: '45deg' }] }]} />
          <View style={[styles.gearTooth, { backgroundColor: color, width: stroke * 1.7, height: size * 0.17, borderRadius: stroke, left: size * 0.13, top: size * 0.13, transform: [{ rotate: '-45deg' }] }]} />
          <View style={[styles.gearTooth, { backgroundColor: color, width: stroke * 1.7, height: size * 0.17, borderRadius: stroke, right: size * 0.13, bottom: size * 0.13, transform: [{ rotate: '-45deg' }] }]} />
          <View style={[styles.gearTooth, { backgroundColor: color, width: stroke * 1.7, height: size * 0.17, borderRadius: stroke, left: size * 0.13, bottom: size * 0.13, transform: [{ rotate: '45deg' }] }]} />
          <View style={[styles.gearCore, { borderColor: color, borderWidth: stroke * 1.45, width: size * 0.58, height: size * 0.58, borderRadius: size * 0.29 }]} />
          <View style={[styles.gearCenterDot, { borderColor: color, borderWidth: stroke, width: size * 0.2, height: size * 0.2, borderRadius: size * 0.1 }]} />
        </View>
      );

    case 'plus':
      // AntDesign 'plus' / Google Material 'add'
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          <View style={[styles.absoluteCenter, { backgroundColor: color, width: size * 0.7, height: stroke }]} />
          <View style={[styles.absoluteCenter, { backgroundColor: color, width: stroke, height: size * 0.7 }]} />
        </View>
      );

    case 'delete':
      // AntDesign 'delete' / Google Material 'delete_outline'
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          {/* Lid */}
          <View style={{ backgroundColor: color, width: size * 0.7, height: stroke, borderRadius: stroke / 2 }} />
          <View style={{ backgroundColor: color, width: size * 0.3, height: stroke, borderTopLeftRadius: stroke / 2, borderTopRightRadius: stroke / 2 }} />
          {/* Body */}
          <View style={{ borderColor: color, borderWidth: stroke, borderTopWidth: 0, width: size * 0.6, height: size * 0.6, borderBottomLeftRadius: stroke, borderBottomRightRadius: stroke, top: 1 }} />
        </View>
      );

    case 'save':
      // AntDesign 'save' / Google Material 'save'
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          <View style={{ borderColor: color, borderWidth: stroke, width: size * 0.75, height: size * 0.75, borderRadius: stroke, justifyContent: 'space-between', padding: stroke * 0.5 }}>
            <View style={{ backgroundColor: color, height: stroke * 1.5, width: '70%' }} />
            <View style={{ borderColor: color, borderWidth: stroke, height: stroke * 2, width: '90%', alignSelf: 'center' }} />
          </View>
        </View>
      );

    case 'play':
      // AntDesign 'play-circle' / Google Material 'play_arrow'
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          <View style={{
            width: 0,
            height: 0,
            borderTopWidth: size * 0.35,
            borderBottomWidth: size * 0.35,
            borderLeftWidth: size * 0.6,
            borderTopColor: 'transparent',
            borderBottomColor: 'transparent',
            borderLeftColor: color,
            left: stroke
          }} />
        </View>
      );

    case 'refresh':
      // Google Material 'refresh'
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          <View style={[styles.circleOutline, { borderColor: color, borderWidth: stroke, width: size * 0.8, height: size * 0.8, borderRadius: size * 0.4 }]}>
            <View style={[styles.syncGap, { right: -stroke, top: -stroke, height: half }]} />
            <View style={[styles.syncArrow, { borderTopColor: color, borderTopWidth: half * 0.5, borderLeftWidth: half * 0.4, borderRightWidth: half * 0.4, top: -half * 0.2, right: -half * 0.3 }]} />
          </View>
        </View>
      );

    case 'chevron-down':
      // Chevron-down angle bracket
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          <View style={{
            borderColor: color,
            borderBottomWidth: stroke * 1.5,
            borderRightWidth: stroke * 1.5,
            width: size * 0.4,
            height: size * 0.4,
            transform: [{ rotate: '45deg' }],
            top: -size * 0.1
          }} />
        </View>
      );

    case 'chevron-right':
      // Chevron-right angle bracket
      return (
        <View style={{ width: size, height: size, justifyContent: 'center', alignItems: 'center' }}>
          <View style={{
            borderColor: color,
            borderTopWidth: stroke * 1.5,
            borderRightWidth: stroke * 1.5,
            width: size * 0.4,
            height: size * 0.4,
            transform: [{ rotate: '45deg' }],
            left: -size * 0.05
          }} />
        </View>
      );

    default:
      return null;
  }
};

const styles = StyleSheet.create({
  homeRoof: {
    width: 0,
    height: 0,
    borderStyle: 'solid',
    borderLeftColor: 'transparent',
    borderRightColor: 'transparent',
  },
  homeBody: {
    borderStyle: 'solid',
  },
  circleOutline: {
    justifyContent: 'center',
    alignItems: 'center',
  },
  iconCanvas: {
    justifyContent: 'center',
    alignItems: 'center',
    position: 'relative',
  },
  autoArrowHead: {
    position: 'absolute',
    width: 0,
    height: 0,
    borderTopColor: 'transparent',
    borderBottomColor: 'transparent',
  },
  autoDot: {
    position: 'absolute',
  },
  syncGap: {
    position: 'absolute',
    width: 6,
    backgroundColor: '#090C15', // Matches fallback background
  },
  syncArrow: {
    position: 'absolute',
    width: 0,
    height: 0,
    borderLeftColor: 'transparent',
    borderRightColor: 'transparent',
  },
  row: {
    flexDirection: 'row',
    justifyContent: 'space-evenly',
    alignItems: 'center',
  },
  sliderTrack: {
    alignItems: 'center',
    position: 'relative',
  },
  sliderKnob: {
    position: 'absolute',
  },
  arrowContainer: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  arrowShaft: {},
  arrowHeadRight: {
    width: 0,
    height: 0,
    borderTopColor: 'transparent',
    borderBottomColor: 'transparent',
  },
  arrowHeadLeft: {
    width: 0,
    height: 0,
    borderTopColor: 'transparent',
    borderBottomColor: 'transparent',
  },
  ioBus: {
    position: 'absolute',
    borderRadius: 2,
  },
  ioLine: {
    position: 'absolute',
    borderRadius: 2,
  },
  ioPort: {
    position: 'absolute',
  },
  clockHandHour: {
    position: 'absolute',
    borderRadius: 2,
  },
  clockHandMinute: {
    position: 'absolute',
    borderRadius: 2,
  },
  gearBody: {
    justifyContent: 'center',
    alignItems: 'center',
    position: 'relative',
  },
  gearTeeth: {
    position: 'absolute',
    height: '100%',
    borderRadius: 1,
  },
  gearHole: {
    position: 'absolute',
    backgroundColor: '#090C15', // Matches fallback background
  },
  gearTooth: {
    position: 'absolute',
  },
  gearCore: {
    position: 'absolute',
    backgroundColor: '#151B24',
  },
  gearCenterDot: {
    position: 'absolute',
  },
  absoluteCenter: {
    position: 'absolute',
  },
});
