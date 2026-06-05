import React, {useEffect, useMemo, useState} from 'react';
import {Pressable, ScrollView, StyleSheet, Text, TextInput, View} from 'react-native';
import {Icon} from '../components/Icon';
import {colors, radius, spacing, typography} from '../styles/theme';
import type {LineConfig} from '../types/plc';

interface Props {
  lines: LineConfig[];
  selectedLineId: string;
  onAddLine(): LineConfig;
  onSaveLine(line: LineConfig): void;
  onDeleteLine(lineId: string): LineConfig[];
  onSelectLine?(lineId: string): void;
}

interface LineDraft {
  id: string;
  name: string;
  host: string;
  port: string;
  slaveId: string;
  pollIntervalMs: string;
  connectionType?: LineConfig['connectionType'];
}

const toDraft = (line: LineConfig): LineDraft => ({
  id: line.id,
  name: line.name,
  host: line.host,
  port: String(line.port),
  slaveId: String(line.slaveId),
  pollIntervalMs: String(line.pollIntervalMs),
  connectionType: line.connectionType,
});

const toInteger = (value: string) => Number(value.trim());

const validateDraft = (draft: LineDraft) => {
  const port = toInteger(draft.port);
  const slaveId = toInteger(draft.slaveId);
  const pollIntervalMs = toInteger(draft.pollIntervalMs);

  if (!draft.name.trim()) {
    return 'Nhập tên line';
  }
  if (!draft.host.trim()) {
    return 'Nhập IP PLC';
  }
  if (!Number.isInteger(port) || port < 1 || port > 65535) {
    return 'Port phải từ 1 đến 65535';
  }
  if (!Number.isInteger(slaveId) || slaveId < 1 || slaveId > 247) {
    return 'Slave ID phải từ 1 đến 247';
  }
  if (!Number.isInteger(pollIntervalMs) || pollIntervalMs < 50 || pollIntervalMs > 5000) {
    return 'Chu kỳ đọc phải từ 50 đến 5000 ms';
  }
  return '';
};

const draftToLine = (draft: LineDraft): LineConfig => ({
  id: draft.id,
  name: draft.name.trim(),
  host: draft.host.trim(),
  port: toInteger(draft.port),
  slaveId: toInteger(draft.slaveId),
  pollIntervalMs: toInteger(draft.pollIntervalMs),
  ...(draft.connectionType ? {connectionType: draft.connectionType} : {}),
});

interface FieldProps {
  label: string;
  value: string;
  keyboardType?: 'default' | 'numeric';
  onChangeText(value: string): void;
}

const Field = ({label, value, keyboardType = 'default', onChangeText}: FieldProps) => (
  <View style={styles.field}>
    <Text style={styles.fieldLabel}>{label}</Text>
    <TextInput
      keyboardType={keyboardType}
      onChangeText={onChangeText}
      selectTextOnFocus
      style={styles.input}
      value={value}
    />
  </View>
);

export const SettingsPage = ({
  lines,
  selectedLineId,
  onAddLine,
  onSaveLine,
  onDeleteLine,
  onSelectLine,
}: Props) => {
  const [editingLineId, setEditingLineId] = useState(selectedLineId);
  const editingLine = lines.find(line => line.id === editingLineId) ?? lines[0];
  const [draft, setDraft] = useState(() => toDraft(editingLine));
  const validationError = useMemo(() => validateDraft(draft), [draft]);
  const canDelete = lines.length > 1;

  useEffect(() => {
    if (!editingLine) {
      return;
    }
    setEditingLineId(editingLine.id);
    setDraft(toDraft(editingLine));
  }, [editingLine]);

  const updateDraft = (key: keyof LineDraft) => (value: string) => {
    setDraft(current => ({...current, [key]: value}));
  };

  const handleAddLine = () => {
    const line = onAddLine();
    setEditingLineId(line.id);
    setDraft(toDraft(line));
    onSelectLine?.(line.id);
  };

  const handleSaveLine = () => {
    if (!validationError) {
      onSaveLine(draftToLine(draft));
    }
  };

  const handleDeleteLine = () => {
    if (!canDelete || !editingLine) {
      return;
    }
    const nextLines = onDeleteLine(editingLine.id);
    const nextLine = nextLines[0];
    setEditingLineId(nextLine.id);
    setDraft(toDraft(nextLine));
    onSelectLine?.(nextLine.id);
  };

  const handleSelectLine = (line: LineConfig) => {
    setEditingLineId(line.id);
    onSelectLine?.(line.id);
  };

  return (
    <View style={styles.root}>
      <View style={styles.content}>
        <View style={styles.rail}>
          <View style={styles.railHeader}>
            <View>
              <Text style={styles.sectionTitle}>Line</Text>
              <Text style={styles.railSubtitle}>{lines.length} cấu hình</Text>
            </View>
            <Pressable
              accessibilityRole="button"
              onPress={handleAddLine}
              style={({pressed}) => [styles.addButton, pressed && styles.buttonPressed]}>
              <Icon name="plus" color={colors.primaryStrong} size={16} />
              <Text style={styles.addButtonText}>Thêm line</Text>
            </Pressable>
          </View>
          <ScrollView contentContainerStyle={styles.lineList}>
            {lines.map(line => {
              const selected = line.id === editingLine.id;
              const active = line.id === selectedLineId;
              return (
                <Pressable
                  key={line.id}
                  accessibilityLabel={`${line.name}${active ? ', active line' : ''}`}
                  accessibilityRole="button"
                  onPress={() => handleSelectLine(line)}
                  style={({pressed}) => [
                    styles.lineButton,
                    selected && styles.lineButtonSelected,
                    active && styles.lineButtonActive,
                    pressed && styles.lineButtonPressed,
                  ]}>
                  <View style={styles.lineNameRow}>
                    <Text style={[styles.lineName, (selected || active) && styles.lineNameSelected]} numberOfLines={1}>
                      {line.name}
                    </Text>
                    {active ? <Text style={styles.activePill}>ACTIVE</Text> : null}
                  </View>
                  <Text style={styles.lineEndpoint} numberOfLines={1}>
                    {line.host}:{line.port}
                  </Text>
                </Pressable>
              );
            })}
          </ScrollView>
        </View>

        <ScrollView style={styles.formPanel} contentContainerStyle={styles.formContent}>
          <View style={styles.formHeader}>
            <Text style={styles.formTitle}>{editingLine.name}</Text>
            <Text style={styles.formMeta}>Slave {editingLine.slaveId}</Text>
          </View>

          <View style={styles.formGrid}>
            <Field label="Tên line" value={draft.name} onChangeText={updateDraft('name')} />
            <Field label="IP PLC" value={draft.host} onChangeText={updateDraft('host')} />
            <Field label="Port" keyboardType="numeric" value={draft.port} onChangeText={updateDraft('port')} />
            <Field label="Slave ID" keyboardType="numeric" value={draft.slaveId} onChangeText={updateDraft('slaveId')} />
            <Field label="Chu kỳ đọc (ms)" keyboardType="numeric" value={draft.pollIntervalMs} onChangeText={updateDraft('pollIntervalMs')} />
          </View>

          <View style={styles.actionRow}>
            <Pressable
              accessibilityRole="button"
              disabled={Boolean(validationError)}
              onPress={handleSaveLine}
              style={({pressed}) => [
                styles.saveButton,
                Boolean(validationError) && styles.disabledButton,
                pressed && !validationError && styles.buttonPressed,
              ]}>
              <Icon name="save" color={validationError ? colors.textDisabled : colors.running} size={18} />
              <Text style={[styles.saveButtonText, Boolean(validationError) && styles.disabledText]}>Lưu line</Text>
            </Pressable>
            <Pressable
              accessibilityRole="button"
              disabled={!canDelete}
              onPress={handleDeleteLine}
              style={({pressed}) => [
                styles.deleteButton,
                !canDelete && styles.disabledButton,
                pressed && canDelete && styles.deleteButtonPressed,
              ]}>
              <Icon name="delete" color={canDelete ? colors.fault : colors.textDisabled} size={18} />
              <Text style={[styles.deleteButtonText, !canDelete && styles.disabledText]}>Xóa line</Text>
            </Pressable>
          </View>

          {validationError ? <Text style={styles.validationText}>{validationError}</Text> : null}
        </ScrollView>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  root: {
    flex: 1,
  },
  addButton: {
    minWidth: 112,
    minHeight: 44,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.primary,
    borderRadius: radius.button,
    backgroundColor: colors.primarySurface,
    paddingHorizontal: spacing.md,
    gap: spacing.xs,
  },
  addButtonText: {
    color: colors.primaryStrong,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  content: {
    flex: 1,
    minHeight: 0,
    flexDirection: 'row',
    gap: spacing.lg,
  },
  rail: {
    width: 320,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.panel,
    padding: spacing.lg,
    gap: spacing.md,
  },
  railHeader: {
    minHeight: 48,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.sm,
  },
  sectionTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 22,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  railSubtitle: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  lineList: {
    gap: spacing.md,
    paddingBottom: spacing.lg,
  },
  lineButton: {
    minHeight: 82,
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.button,
    backgroundColor: colors.recessed,
    paddingHorizontal: spacing.md,
    gap: spacing.xs,
  },
  lineButtonSelected: {
    borderColor: colors.primary,
    backgroundColor: colors.primarySurface,
    borderLeftWidth: 4,
  },
  lineButtonActive: {
    borderColor: colors.running,
  },
  lineButtonPressed: {
    borderColor: colors.primary,
  },
  lineNameRow: {
    minHeight: 24,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.sm,
  },
  lineName: {
    flex: 1,
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 18,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  lineNameSelected: {
    color: colors.primaryStrong,
  },
  activePill: {
    minWidth: 58,
    borderWidth: 1,
    borderColor: colors.running,
    borderRadius: radius.badge,
    backgroundColor: colors.runningSurface,
    color: colors.running,
    fontFamily: typography.fontFamily,
    fontSize: 10,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textAlign: 'center',
    paddingHorizontal: spacing.xs,
    paddingVertical: 3,
  },
  lineEndpoint: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 13,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  formPanel: {
    flex: 1,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.panel,
  },
  formContent: {
    padding: spacing.lg,
    gap: spacing.lg,
  },
  formHeader: {
    minHeight: 48,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  formTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 24,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  formMeta: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 15,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  formGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.lg,
  },
  field: {
    width: '31.8%',
    minWidth: 260,
    gap: spacing.xs,
  },
  fieldLabel: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  input: {
    minHeight: 54,
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.recessed,
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 18,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
  },
  actionRow: {
    flexDirection: 'row',
    gap: spacing.md,
  },
  saveButton: {
    minWidth: 170,
    minHeight: 56,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.running,
    borderRadius: radius.button,
    backgroundColor: colors.runningSurface,
    paddingHorizontal: spacing.lg,
    gap: spacing.sm,
  },
  saveButtonText: {
    color: colors.running,
    fontFamily: typography.fontFamily,
    fontSize: 17,
    fontWeight: typography.weights.semibold,
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
  deleteButton: {
    minWidth: 150,
    minHeight: 56,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.fault,
    borderRadius: radius.button,
    backgroundColor: colors.faultSurface,
    paddingHorizontal: spacing.lg,
    gap: spacing.sm,
  },
  deleteButtonPressed: {
    backgroundColor: colors.panel,
  },
  deleteButtonText: {
    color: colors.fault,
    fontFamily: typography.fontFamily,
    fontSize: 17,
    fontWeight: typography.weights.semibold,
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
  disabledButton: {
    borderColor: colors.borderStrong,
    backgroundColor: colors.recessed,
    opacity: 0.45,
  },
  disabledText: {
    color: colors.textDisabled,
  },
  buttonPressed: {
    backgroundColor: colors.panel,
  },
  validationText: {
    color: colors.fault,
    fontFamily: typography.fontFamily,
    fontSize: 15,
    fontWeight: typography.weights.medium,
  },
});
