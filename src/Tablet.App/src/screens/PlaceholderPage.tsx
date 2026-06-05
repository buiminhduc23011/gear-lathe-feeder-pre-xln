import React from 'react';
import {StyleSheet, Text, View} from 'react-native';
import {colors, radius, spacing} from '../styles/theme';

interface Props {
  title: string;
}

export const PlaceholderPage = ({title}: Props) => (
  <View style={styles.container}>
    <View style={styles.statusGrid}>
      <View style={styles.statusTile}>
        <Text style={styles.statusLabel}>STATE</Text>
        <Text style={styles.statusValue}>RESERVED</Text>
      </View>
      <View style={styles.statusTile}>
        <Text style={styles.statusLabel}>CONTROL</Text>
        <Text style={styles.statusValue}>LOCKED</Text>
      </View>
      <View style={styles.statusTileWide}>
        <Text style={styles.statusLabel}>NEXT STEP</Text>
        <Text style={styles.statusValue}>MAP PLC TAGS</Text>
      </View>
    </View>
  </View>
);

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  statusGrid: {
    flexDirection: 'row',
    gap: spacing.lg,
  },
  statusTile: {
    width: 180,
    minHeight: 92,
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.card,
    backgroundColor: colors.panel,
    paddingHorizontal: spacing.lg,
    gap: spacing.xs,
  },
  statusTileWide: {
    width: 260,
    minHeight: 92,
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.card,
    backgroundColor: colors.panel,
    paddingHorizontal: spacing.lg,
    gap: spacing.xs,
  },
  statusLabel: {
    color: colors.textMuted,
    fontSize: 10,
    fontWeight: '600',
    letterSpacing: 0,
  },
  statusValue: {
    color: colors.text,
    fontFamily: 'monospace',
    fontSize: 17,
    fontWeight: '600',
    letterSpacing: 0,
  },
});
