import React, {useEffect, useMemo, useRef, useState} from 'react';
import {
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  useWindowDimensions,
  View,
} from 'react-native';
import {HmiButton, HmiModal} from '../components/AntdCompat';
import {Icon} from '../components/Icon';
import {colors, radius, spacing, typography} from '../styles/theme';
import type {LineConfig} from '../types/plc';
import {OTA_CHECK_URL, updateService} from '../services/UpdateService';

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

type ModalTarget = 'line' | 'update' | null;

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
  const {width} = useWindowDimensions();
  const dense = width <= 820;
  const [focusedLineId, setFocusedLineId] = useState(selectedLineId);
  const previousSelectedLineIdRef = useRef(selectedLineId);
  const [modalTarget, setModalTarget] = useState<ModalTarget>(null);
  const focusedLine = lines.find(line => line.id === focusedLineId) ?? lines[0];
  const [draft, setDraft] = useState(() => toDraft(focusedLine));
  const validationError = useMemo(() => validateDraft(draft), [draft]);
  const canDelete = lines.length > 1;

  const [currentVersion, setCurrentVersion] = useState({versionName: '1.0.0', versionCode: 100});
  const [updateInfo, setUpdateInfo] = useState<{
    checked: boolean;
    hasUpdate: boolean;
    latestVersion: string;
    latestVersionCode: number | null;
    changelog: string;
    downloadUrl: string | null;
  } | null>(null);
  const [checking, setChecking] = useState(false);
  const [downloadProgress, setDownloadProgress] = useState<number | null>(null);
  const [downloadStatus, setDownloadStatus] = useState<string>('');
  const [errorMessage, setErrorMessage] = useState<string>('');
  const [hasInstallPermission, setHasInstallPermission] = useState<boolean>(true);

  useEffect(() => {
    updateService.getAppVersion().then(ver => {
      setCurrentVersion(ver);
    });
    updateService.checkInstallPermission().then(allowed => {
      setHasInstallPermission(allowed);
    });
  }, []);

  useEffect(() => {
    if (previousSelectedLineIdRef.current === selectedLineId) {
      return;
    }
    previousSelectedLineIdRef.current = selectedLineId;
    if (lines.some(line => line.id === selectedLineId)) {
      setFocusedLineId(selectedLineId);
    }
  }, [lines, selectedLineId]);

  useEffect(() => {
    if (!lines.some(line => line.id === focusedLineId) && lines[0]) {
      setFocusedLineId(lines[0].id);
    }
  }, [focusedLineId, lines]);

  useEffect(() => {
    if (modalTarget !== 'line' && focusedLine) {
      setDraft(toDraft(focusedLine));
    }
  }, [focusedLine, modalTarget]);

  const updateDraft = (key: keyof LineDraft) => (value: string) => {
    setDraft(current => ({...current, [key]: value}));
  };

  const closeModal = () => {
    setModalTarget(null);
    if (focusedLine) {
      setDraft(toDraft(focusedLine));
    }
  };

  const openLineModal = (line: LineConfig) => {
    setFocusedLineId(line.id);
    setDraft(toDraft(line));
    setModalTarget('line');
  };

  const handleActivateLine = (line: LineConfig) => {
    setFocusedLineId(line.id);
    onSelectLine?.(line.id);
  };

  const handleAddLine = () => {
    const line = onAddLine();
    setFocusedLineId(line.id);
    setDraft(toDraft(line));
    setModalTarget('line');
    onSelectLine?.(line.id);
  };

  const handleSaveLine = () => {
    if (validationError) {
      return;
    }
    const savedLine = draftToLine(draft);
    onSaveLine(savedLine);
    setFocusedLineId(savedLine.id);
    setModalTarget(null);
  };

  const handleDeleteLine = (lineId = focusedLine?.id) => {
    if (!canDelete || !lineId) {
      return;
    }
    const nextLines = onDeleteLine(lineId);
    const nextLine = nextLines.find(line => line.id === focusedLineId) ?? nextLines[0];
    if (nextLine) {
      setFocusedLineId(nextLine.id);
      setDraft(toDraft(nextLine));
      if (selectedLineId === lineId) {
        onSelectLine?.(nextLine.id);
      }
    }
    setModalTarget(null);
  };

  const handleCheckUpdate = async () => {
    setChecking(true);
    setErrorMessage('');
    try {
      const result = await updateService.checkUpdate(OTA_CHECK_URL);
      setUpdateInfo({
        checked: true,
        hasUpdate: result.hasUpdate,
        latestVersion: result.latestVersion,
        latestVersionCode: result.latestVersionCode,
        changelog: result.changelog,
        downloadUrl: result.downloadUrl,
      });
    } catch (err: any) {
      setErrorMessage(err.message || 'Lỗi khi kết nối tới máy chủ cập nhật');
    } finally {
      setChecking(false);
    }
  };

  const handleStartUpdate = async () => {
    if (!updateInfo || !updateInfo.downloadUrl) {
      return;
    }

    const hasPermission = await updateService.checkInstallPermission();
    if (!hasPermission) {
      setHasInstallPermission(false);
      updateService.openInstallPermissionSettings();
      setErrorMessage('Vui lòng cấp quyền cài đặt ứng dụng từ nguồn không xác định và thử lại');
      return;
    }

    setErrorMessage('');
    setDownloadProgress(0);
    setDownloadStatus('Đang bắt đầu tải về...');

    try {
      await updateService.downloadAndInstall(updateInfo.downloadUrl, event => {
        if (event.status === 'downloading') {
          setDownloadProgress(event.progress ?? 0);
          setDownloadStatus(`Đang tải về: ${event.progress}%`);
        } else if (event.status === 'done') {
          setDownloadProgress(100);
          setDownloadStatus('Đang chuẩn bị cài đặt...');
        } else if (event.status === 'error') {
          setDownloadProgress(null);
          setDownloadStatus('');
          setErrorMessage(event.message || 'Lỗi tải file APK');
        }
      });
    } catch (err: any) {
      setDownloadProgress(null);
      setDownloadStatus('');
      setErrorMessage(err.message || 'Lỗi trong quá trình cập nhật');
    }
  };

  return (
    <View style={styles.root}>
      <View style={styles.tablePanel}>
        <View style={styles.tableToolbar}>
          <View>
            <Text style={styles.sectionTitle}>Danh sách PLC</Text>
            <Text style={styles.railSubtitle}>{lines.length} cấu hình</Text>
          </View>
          <View style={styles.toolbarActions}>
            <HmiButton
              accessibilityRole="button"
              onPress={() => setModalTarget('update')}
              activeStyle={styles.buttonPressed}
              style={styles.cornerButton}>
              <Icon name="refresh" color={colors.text} size={15} />
              <Text style={styles.cornerButtonText}>Cập nhật</Text>
            </HmiButton>
            <HmiButton
              accessibilityRole="button"
              onPress={handleAddLine}
              activeStyle={styles.buttonPressed}
              style={styles.cornerButton}>
              <Icon name="plus" color={colors.text} size={15} />
              <Text style={styles.cornerButtonText}>Thêm line</Text>
            </HmiButton>
          </View>
        </View>

        <View style={styles.plcTable}>
          <View style={[styles.plcRow, styles.plcHeaderRow]}>
            <Text style={[styles.plcHeaderText, styles.colName]}>Tên Line</Text>
            <Text style={[styles.plcHeaderText, styles.colType]}>Loại PLC</Text>
            <Text style={[styles.plcHeaderText, styles.colIp]}>IP</Text>
            <Text style={[styles.plcHeaderText, styles.colPort]}>Port</Text>
            <Text style={[styles.plcHeaderText, styles.colSlave]}>Slave ID</Text>
            <Text style={[styles.plcHeaderText, styles.colPoll]}>Chu kỳ đọc</Text>
            <Text style={[styles.plcHeaderText, styles.colActions]}>Thao tác</Text>
          </View>

          <ScrollView style={styles.plcTableBody} contentContainerStyle={styles.plcTableBodyContent}>
            {lines.map(line => {
              const active = line.id === selectedLineId;
              return (
                <View key={line.id} style={[styles.plcRow, active && styles.plcRowActive]}>
                  <Text style={[styles.plcCellText, styles.colName]} numberOfLines={1}>
                    {line.name}
                  </Text>
                  <Text style={[styles.plcCellText, styles.colType]} numberOfLines={1}>
                    {line.connectionType ?? 'TcpAS'}
                  </Text>
                  <Text style={[styles.plcCellText, styles.colIp]} numberOfLines={1}>
                    {line.host}
                  </Text>
                  <Text style={[styles.plcCellText, styles.colPort]} numberOfLines={1}>
                    {line.port}
                  </Text>
                  <Text style={[styles.plcCellText, styles.colSlave]} numberOfLines={1}>
                    {line.slaveId}
                  </Text>
                  <Text style={[styles.plcCellText, styles.colPoll]} numberOfLines={1}>
                    {line.pollIntervalMs} ms
                  </Text>
                  <View style={[styles.rowActions, styles.colActions]}>
                    <HmiButton
                      accessibilityLabel={`Active ${line.name}`}
                      accessibilityRole="button"
                      disabled={active}
                      onPress={() => handleActivateLine(line)}
                      activeStyle={styles.buttonPressed}
                      style={[styles.rowActionButton, active && styles.rowActionActive] as any}>
                      <Text style={[styles.rowActionText, active && styles.rowActionTextActive]}>
                        {active ? 'Active' : 'Active'}
                      </Text>
                    </HmiButton>
                    <HmiButton
                      accessibilityLabel={`Sửa ${line.name}`}
                      accessibilityRole="button"
                      onPress={() => openLineModal(line)}
                      activeStyle={styles.buttonPressed}
                      style={styles.rowActionButton}>
                      <Text style={styles.rowActionText}>Sửa</Text>
                    </HmiButton>
                    <HmiButton
                      accessibilityLabel={`Xóa ${line.name}`}
                      accessibilityRole="button"
                      disabled={!canDelete}
                      onPress={() => handleDeleteLine(line.id)}
                      activeStyle={styles.buttonPressed}
                      style={[styles.rowActionButton, styles.rowActionDanger, !canDelete && styles.disabledButton] as any}>
                      <Text style={[styles.rowActionText, !canDelete && styles.disabledText]}>Xóa</Text>
                    </HmiButton>
                  </View>
                </View>
              );
            })}
          </ScrollView>
        </View>
      </View>

      <HmiModal
        animationType="fade"
        bodyStyle={styles.HmiModalBody}
        maskClosable
        onClose={closeModal}
        onRequestClose={() => {
          closeModal();
          return true;
        }}
        style={[styles.modalPanel, dense && styles.modalPanelDense] as any}
        transparent
        visible={modalTarget === 'line'}>
        <ScrollView contentContainerStyle={[styles.modalContent, dense && styles.modalContentDense]}>
              <View style={styles.modalHeader}>
                <View>
                  <Text style={styles.modalEyebrow}>Line setting</Text>
                  <Text style={styles.modalTitle}>{draft.name || focusedLine.name}</Text>
                </View>
                <HmiButton
                  accessibilityLabel="Đóng modal cài đặt line"
                  accessibilityRole="button"
                  onPress={closeModal}
                  activeStyle={styles.buttonPressed}
                  style={styles.closeButton}>
                  <Text style={styles.closeButtonText}>Đóng</Text>
                </HmiButton>
              </View>

              <View style={styles.formGrid}>
                <Field label="Tên line" value={draft.name} onChangeText={updateDraft('name')} />
                <Field label="IP PLC" value={draft.host} onChangeText={updateDraft('host')} />
                <Field label="Port" keyboardType="numeric" value={draft.port} onChangeText={updateDraft('port')} />
                <Field label="Slave ID" keyboardType="numeric" value={draft.slaveId} onChangeText={updateDraft('slaveId')} />
                <Field
                  label="Chu kỳ đọc (ms)"
                  keyboardType="numeric"
                  value={draft.pollIntervalMs}
                  onChangeText={updateDraft('pollIntervalMs')}
                />
              </View>

              {validationError ? <Text style={styles.validationText}>{validationError}</Text> : null}

              <View style={styles.actionRow}>
                <HmiButton
                  accessibilityRole="button"
                  disabled={Boolean(validationError)}
                  onPress={handleSaveLine}
                  activeStyle={styles.buttonPressed}
                  style={[
                    styles.saveButton,
                    Boolean(validationError) && styles.disabledButton,
                  ] as any}>
                  <Icon name="save" color={validationError ? colors.textDisabled : colors.text} size={18} />
                  <Text style={[styles.saveButtonText, Boolean(validationError) && styles.disabledText]}>Lưu line</Text>
                </HmiButton>
                <HmiButton
                  accessibilityRole="button"
                  disabled={!canDelete}
                  onPress={() => handleDeleteLine()}
                  activeStyle={styles.buttonPressed}
                  style={[
                    styles.deleteButton,
                    !canDelete && styles.disabledButton,
                  ] as any}>
                  <Icon name="delete" color={canDelete ? colors.text : colors.textDisabled} size={18} />
                  <Text style={[styles.deleteButtonText, !canDelete && styles.disabledText]}>Xóa line</Text>
                </HmiButton>
              </View>
            </ScrollView>
      </HmiModal>

      <HmiModal
        animationType="fade"
        bodyStyle={styles.HmiModalBody}
        maskClosable
        onClose={closeModal}
        onRequestClose={() => {
          closeModal();
          return true;
        }}
        style={[styles.modalPanel, styles.updateModalPanel, dense && styles.modalPanelDense] as any}
        transparent
        visible={modalTarget === 'update'}>
        <ScrollView contentContainerStyle={[styles.modalContent, dense && styles.modalContentDense]}>
              <View style={styles.modalHeader}>
                <View>
                  <Text style={styles.modalEyebrow}>System update</Text>
                  <Text style={styles.modalTitle}>Cập nhật hệ thống</Text>
                </View>
                <HmiButton
                  accessibilityLabel="Đóng modal cập nhật hệ thống"
                  accessibilityRole="button"
                  onPress={closeModal}
                  activeStyle={styles.buttonPressed}
                  style={styles.closeButton}>
                  <Text style={styles.closeButtonText}>Đóng</Text>
                </HmiButton>
              </View>

              <View style={styles.updateStatusCard}>
                <View style={styles.versionRow}>
                  <View style={styles.versionBlock}>
                    <Text style={styles.versionLabel}>Phiên bản hiện tại</Text>
                    <Text style={styles.versionValue}>v{currentVersion.versionName} ({currentVersion.versionCode})</Text>
                  </View>
                  {updateInfo?.hasUpdate ? (
                    <View style={styles.versionBlock}>
                      <Text style={styles.versionLabel}>Phiên bản mới nhất</Text>
                      <Text style={styles.versionValue}>
                        {updateInfo.latestVersion}
                        {updateInfo.latestVersionCode !== null ? ` (${updateInfo.latestVersionCode})` : ''}
                      </Text>
                    </View>
                  ) : null}
                </View>

                <View style={styles.statusRow}>
                  <Text style={styles.statusLabel}>Trạng thái: </Text>
                  {checking ? (
                    <Text style={styles.statusValue}>Đang kiểm tra cập nhật...</Text>
                  ) : downloadProgress !== null ? (
                    <Text style={styles.statusValue}>{downloadStatus}</Text>
                  ) : updateInfo?.hasUpdate ? (
                    <Text style={styles.statusValue}>Có phiên bản mới khả dụng</Text>
                  ) : updateInfo?.checked ? (
                    <Text style={styles.statusValue}>Ứng dụng đã được cập nhật mới nhất</Text>
                  ) : (
                    <Text style={styles.statusValue}>Chưa kiểm tra</Text>
                  )}
                </View>
              </View>

              {errorMessage ? (
                <View style={styles.errorBox}>
                  <Text style={styles.errorText}>{errorMessage}</Text>
                </View>
              ) : null}

              {downloadProgress !== null ? (
                <View style={styles.progressContainer}>
                  <View style={styles.progressBarBg}>
                    <View style={[styles.progressBarFill, {width: `${downloadProgress}%`}]} />
                  </View>
                  <Text style={styles.progressPercent}>{downloadProgress}%</Text>
                </View>
              ) : null}

              {updateInfo?.hasUpdate && downloadProgress === null ? (
                <View style={styles.changelogBox}>
                  <Text style={styles.changelogTitle}>Nhật ký cập nhật</Text>
                  <ScrollView style={styles.changelogScroll} contentContainerStyle={styles.changelogContent}>
                    <Text style={styles.changelogText}>{updateInfo.changelog}</Text>
                  </ScrollView>
                </View>
              ) : null}

              <View style={styles.actionRow}>
                {downloadProgress === null ? (
                  <HmiButton
                    accessibilityRole="button"
                    disabled={checking}
                    onPress={handleCheckUpdate}
                    activeStyle={styles.buttonPressed}
                    style={[
                      styles.checkButton,
                      checking && styles.disabledButton,
                    ] as any}>
                    <Icon name="refresh" color={colors.text} size={18} />
                    <Text style={styles.checkButtonText}>Kiểm tra cập nhật</Text>
                  </HmiButton>
                ) : null}

                {updateInfo?.hasUpdate && downloadProgress === null ? (
                  <HmiButton
                    accessibilityRole="button"
                    onPress={handleStartUpdate}
                    activeStyle={styles.buttonPressed}
                    style={styles.updateButton}>
                    <Icon name="check" color={colors.text} size={18} />
                    <Text style={styles.updateButtonText}>Tải và cài đặt</Text>
                  </HmiButton>
                ) : null}

                {!hasInstallPermission ? (
                  <HmiButton
                    accessibilityRole="button"
                    onPress={() => updateService.openInstallPermissionSettings()}
                    activeStyle={styles.buttonPressed}
                    style={styles.permissionButton}>
                    <Text style={styles.permissionButtonText}>Cấp quyền cài đặt</Text>
                  </HmiButton>
                ) : null}
              </View>
            </ScrollView>
      </HmiModal>
    </View>
  );
};

const styles = StyleSheet.create({
  root: {
    flex: 1,
  },
  tablePanel: {
    flex: 1,
    minHeight: 0,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.panel,
    padding: spacing.md,
    gap: spacing.md,
  },
  tableToolbar: {
    minHeight: 44,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.md,
  },
  toolbarActions: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  cornerButton: {
    minWidth: 98,
    minHeight: 38,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.sm,
    gap: spacing.xs,
  },
  cornerButtonText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  plcTable: {
    flex: 1,
    minHeight: 0,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.recessed,
    overflow: 'hidden',
  },
  plcTableBody: {
    flex: 1,
  },
  plcTableBodyContent: {
    paddingBottom: spacing.xs,
  },
  plcRow: {
    minHeight: 44,
    flexDirection: 'row',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: colors.divider,
    paddingHorizontal: spacing.sm,
    gap: 6,
  },
  plcHeaderRow: {
    minHeight: 36,
    backgroundColor: colors.subpanel,
  },
  plcRowActive: {
    backgroundColor: colors.subpanel,
  },
  plcHeaderText: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 11,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  plcCellText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 13,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  colName: {
    width: 112,
  },
  colType: {
    width: 74,
  },
  colIp: {
    width: 116,
  },
  colPort: {
    width: 52,
  },
  colSlave: {
    width: 68,
  },
  colPoll: {
    width: 78,
  },
  colActions: {
    width: 176,
  },
  rowActions: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'flex-end',
    gap: spacing.xs,
  },
  rowActionButton: {
    minWidth: 44,
    minHeight: 30,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.xs,
  },
  rowActionActive: {
    borderColor: colors.running,
    backgroundColor: colors.recessed,
  },
  rowActionDanger: {
    borderColor: colors.fault,
  },
  rowActionText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 11,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  rowActionTextActive: {
    color: colors.text,
  },
  content: {
    flex: 1,
    minHeight: 0,
    flexDirection: 'row',
    gap: spacing.md,
  },
  contentStacked: {
    flexDirection: 'column',
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
  railDense: {
    width: 252,
    padding: spacing.md,
    gap: spacing.sm,
  },
  railStacked: {
    width: '100%',
    maxHeight: 310,
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
  addButton: {
    minWidth: 112,
    minHeight: 44,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.subpanel,
    paddingHorizontal: spacing.md,
    gap: spacing.xs,
  },
  addButtonText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  lineList: {
    gap: spacing.md,
    paddingBottom: spacing.sm,
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
  lineButtonDense: {
    minHeight: 68,
    paddingHorizontal: spacing.md,
  },
  lineButtonSelected: {
    borderColor: colors.borderStrong,
    backgroundColor: colors.subpanel,
    borderLeftWidth: 4,
  },
  lineButtonActive: {
    borderColor: colors.running,
  },
  lineButtonPressed: {
    borderColor: colors.borderStrong,
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
    color: colors.text,
  },
  activePill: {
    minWidth: 58,
    borderWidth: 1,
    borderColor: colors.running,
    borderRadius: radius.badge,
    backgroundColor: colors.runningSurface,
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 10,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textAlign: 'center',
    paddingHorizontal: spacing.xs,
    paddingVertical: 3,
  },
  lineFooter: {
    minHeight: 20,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.sm,
  },
  lineEndpoint: {
    flex: 1,
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 13,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  overviewPanel: {
    flex: 1,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.panel,
  },
  overviewContent: {
    flex: 1,
    padding: spacing.lg,
    gap: spacing.md,
  },
  overviewContentDense: {
    padding: spacing.md,
    gap: spacing.md,
  },
  overviewHeader: {
    minHeight: 64,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.lg,
  },
  overviewHeaderDense: {
    minHeight: 50,
  },
  overviewEyebrow: {
    color: colors.textMuted,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  overviewTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 30,
    fontWeight: typography.weights.bold,
    letterSpacing: 0,
  },
  overviewTitleDense: {
    fontSize: 24,
  },
  tableTitleRow: {
    minHeight: 54,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.md,
  },
  settingsTable: {
    flex: 1,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.recessed,
    overflow: 'hidden',
  },
  tableHeaderRow: {
    minHeight: 36,
    flexDirection: 'row',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    backgroundColor: colors.subpanel,
    paddingHorizontal: spacing.md,
    gap: spacing.md,
  },
  tableHeaderLabel: {
    flex: 1.1,
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  tableHeaderValue: {
    flex: 1,
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  tableHeaderAction: {
    width: 92,
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textAlign: 'right',
    textTransform: 'uppercase',
  },
  tableRow: {
    minHeight: 44,
    flexDirection: 'row',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: colors.divider,
    paddingHorizontal: spacing.md,
    gap: spacing.md,
  },
  tableRowPressed: {
    backgroundColor: colors.raised,
  },
  tableLabel: {
    flex: 1.1,
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 15,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  tableValue: {
    flex: 1,
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 15,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  tableActionCell: {
    width: 92,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'flex-end',
    gap: spacing.xs,
  },
  tableActionText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 13,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  selectedCard: {
    minHeight: 118,
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.recessed,
    padding: spacing.lg,
    gap: spacing.sm,
  },
  selectedMetaRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.sm,
  },
  neutralPill: {
    minHeight: 30,
    borderWidth: 1,
    borderColor: colors.borderStrong,
    borderRadius: radius.badge,
    backgroundColor: colors.subpanel,
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    paddingHorizontal: spacing.md,
    paddingVertical: 6,
  },
  modalActionGrid: {
    gap: spacing.md,
  },
  modalActionCard: {
    minHeight: 70,
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.subpanel,
    paddingHorizontal: spacing.lg,
    gap: spacing.md,
  },
  modalActionText: {
    flex: 1,
    gap: 2,
  },
  modalActionTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 18,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  modalActionSubtitle: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  primaryAction: {
    width: '100%',
    minHeight: 50,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.lg,
    gap: spacing.sm,
  },
  primaryActionText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 15,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  endpointCard: {
    minHeight: 106,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    borderWidth: 1,
    borderColor: colors.borderStrong,
    borderRadius: radius.panel,
    backgroundColor: colors.recessed,
    padding: spacing.lg,
    gap: spacing.lg,
  },
  endpointCardDense: {
    minHeight: 78,
    padding: spacing.md,
  },
  cardLabel: {
    color: colors.textMuted,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  endpointText: {
    color: colors.primaryStrong,
    fontFamily: typography.fontFamily,
    fontSize: 30,
    fontWeight: typography.weights.bold,
    letterSpacing: 0,
  },
  endpointTextDense: {
    fontSize: 24,
  },
  protocolPill: {
    minWidth: 82,
    borderWidth: 1,
    borderColor: colors.primary,
    borderRadius: radius.badge,
    backgroundColor: colors.primarySurface,
    color: colors.primaryStrong,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textAlign: 'center',
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
  },
  summaryGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.md,
  },
  summaryCell: {
    flexGrow: 1,
    flexBasis: 110,
    minHeight: 72,
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.button,
    backgroundColor: colors.subpanel,
    padding: spacing.md,
    gap: spacing.xs,
  },
  summaryCellAccent: {
    borderColor: colors.primary,
    backgroundColor: colors.primarySurface,
  },
  summaryCellWarning: {
    borderColor: colors.warning,
    backgroundColor: colors.warningSurface,
  },
  summaryLabel: {
    color: colors.textMuted,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  summaryValue: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 20,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  summaryValueAccent: {
    color: colors.primaryStrong,
  },
  summaryValueWarning: {
    color: colors.warning,
  },
  updateCard: {
    minHeight: 82,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.panel,
    backgroundColor: colors.subpanel,
    padding: spacing.md,
    gap: spacing.lg,
  },
  updateCardDense: {
    minHeight: 76,
  },
  updateCardText: {
    flex: 1,
    gap: spacing.xs,
  },
  updateTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 18,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
  },
  updateMeta: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.medium,
    letterSpacing: 0,
  },
  secondaryAction: {
    minWidth: 156,
    minHeight: 48,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.primary,
    borderRadius: radius.button,
    backgroundColor: colors.primarySurface,
    paddingHorizontal: spacing.lg,
    gap: spacing.sm,
  },
  secondaryActionText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  modalBackdrop: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    padding: spacing.xl,
  },
  modalScrim: {
    position: 'absolute',
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.62)',
  },
  modalPanel: {
    width: '100%',
    maxWidth: 720,
    maxHeight: '88%',
    borderWidth: 1,
    borderColor: colors.borderStrong,
    borderRadius: radius.panel,
    backgroundColor: colors.panel,
    shadowColor: '#000000',
    shadowOpacity: 0.45,
    shadowRadius: 24,
    shadowOffset: {width: 0, height: 18},
    elevation: 18,
  },
  updateModalPanel: {
    maxWidth: 820,
  },
  HmiModalBody: {
    backgroundColor: colors.panel,
    paddingHorizontal: 0,
    paddingVertical: 0,
  },
  modalPanelDense: {
    maxHeight: '92%',
  },
  modalContent: {
    padding: spacing.xl,
    gap: spacing.lg,
  },
  modalContentDense: {
    padding: spacing.lg,
    gap: spacing.md,
  },
  modalHeader: {
    minHeight: 52,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.lg,
  },
  modalEyebrow: {
    color: colors.textMuted,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  modalTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 24,
    fontWeight: typography.weights.bold,
    letterSpacing: 0,
  },
  closeButton: {
    minWidth: 82,
    minHeight: 42,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.button,
    backgroundColor: colors.recessed,
    paddingHorizontal: spacing.md,
  },
  closeButtonText: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 13,
    fontWeight: typography.weights.semibold,
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  formGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.lg,
  },
  field: {
    flexGrow: 1,
    flexBasis: 260,
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
    flexWrap: 'wrap',
    gap: spacing.md,
  },
  saveButton: {
    minWidth: 170,
    minHeight: 56,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.lg,
    gap: spacing.sm,
  },
  saveButtonText: {
    color: colors.text,
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
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.lg,
    gap: spacing.sm,
  },
  deleteButtonPressed: {
    backgroundColor: colors.panel,
  },
  deleteButtonText: {
    color: colors.text,
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
  updateStatusCard: {
    backgroundColor: colors.subpanel,
    borderRadius: radius.panel,
    padding: spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
    gap: spacing.md,
  },
  versionRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
    gap: spacing.lg,
  },
  versionBlock: {
    flex: 1,
    minWidth: 220,
    backgroundColor: colors.recessed,
    padding: spacing.md,
    borderRadius: radius.button,
    borderWidth: 1,
    borderColor: colors.border,
  },
  versionLabel: {
    color: colors.textMuted,
    fontFamily: typography.fontFamily,
    fontSize: 12,
    fontWeight: typography.weights.medium,
    marginBottom: spacing.xs,
  },
  versionValue: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 16,
    fontWeight: typography.weights.semibold,
  },
  statusRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: spacing.xs,
  },
  statusLabel: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.medium,
  },
  statusValue: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.semibold,
  },
  errorBox: {
    backgroundColor: colors.faultSurface,
    borderColor: colors.fault,
    borderWidth: 1,
    borderRadius: radius.button,
    padding: spacing.md,
  },
  errorText: {
    color: colors.fault,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.medium,
  },
  progressContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    backgroundColor: colors.subpanel,
    padding: spacing.md,
    borderRadius: radius.button,
    borderWidth: 1,
    borderColor: colors.border,
  },
  progressBarBg: {
    flex: 1,
    height: 12,
    backgroundColor: colors.recessed,
    borderRadius: 6,
    overflow: 'hidden',
  },
  progressBarFill: {
    height: '100%',
    backgroundColor: colors.running,
    borderRadius: 6,
  },
  progressPercent: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 14,
    fontWeight: typography.weights.semibold,
    minWidth: 40,
    textAlign: 'right',
  },
  changelogBox: {
    backgroundColor: colors.subpanel,
    borderRadius: radius.panel,
    padding: spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
    gap: spacing.sm,
    minHeight: 150,
  },
  changelogTitle: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 16,
    fontWeight: typography.weights.semibold,
  },
  changelogScroll: {
    backgroundColor: colors.recessed,
    borderRadius: radius.button,
    borderWidth: 1,
    borderColor: colors.border,
  },
  changelogContent: {
    padding: spacing.md,
  },
  changelogText: {
    color: colors.textSecondary,
    fontFamily: typography.fontFamily,
    fontSize: 13,
    lineHeight: 20,
  },
  checkButton: {
    minWidth: 200,
    minHeight: 56,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.lg,
    gap: spacing.sm,
  },
  checkButtonText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 17,
    fontWeight: typography.weights.semibold,
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
  updateButton: {
    minWidth: 180,
    minHeight: 56,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.lg,
    gap: spacing.sm,
  },
  updateButtonText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 17,
    fontWeight: typography.weights.semibold,
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
  permissionButton: {
    minWidth: 180,
    minHeight: 56,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: colors.borderStrong,
    borderRadius: radius.button,
    backgroundColor: colors.raised,
    paddingHorizontal: spacing.lg,
  },
  permissionButtonText: {
    color: colors.text,
    fontFamily: typography.fontFamily,
    fontSize: 17,
    fontWeight: typography.weights.semibold,
    textTransform: 'uppercase',
    letterSpacing: 0,
  },
});
