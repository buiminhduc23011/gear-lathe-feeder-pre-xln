import type {PlcArea, PlcTagBinding} from './addressing';

export interface PlcReadRange {
  area: PlcArea;
  startAddress: number;
  count: number;
}

const MAX_POINTS_PER_READ = 100;
const MAX_GAP_FILL = 10;

export const buildReadRanges = (bindings: PlcTagBinding[]): PlcReadRange[] => {
  const ranges: PlcReadRange[] = [];
  for (const area of ['coil', 'discrete', 'holding'] as const) {
    const segments = bindings
      .filter(binding => binding.area === area)
      .map(binding => ({
        start: binding.startAddress,
        end: binding.startAddress + binding.span - 1,
      }))
      .sort((left, right) => left.start - right.start);

    if (segments.length === 0) {
      continue;
    }

    const groups: Array<{start: number; end: number}> = [];
    let current = {...segments[0]};
    for (const segment of segments.slice(1)) {
      if (segment.start <= current.end + 1) {
        current.end = Math.max(current.end, segment.end);
      } else {
        groups.push(current);
        current = {...segment};
      }
    }
    groups.push(current);

    const merged: Array<{start: number; end: number}> = [groups[0]];
    for (const group of groups.slice(1)) {
      const last = merged[merged.length - 1];
      const gap = group.start - last.end - 1;
      const combinedCount = group.end - last.start + 1;
      if (gap <= MAX_GAP_FILL && combinedCount <= MAX_POINTS_PER_READ) {
        last.end = group.end;
      } else {
        merged.push({...group});
      }
    }

    for (const range of merged) {
      let offset = 0;
      const total = range.end - range.start + 1;
      while (offset < total) {
        const count = Math.min(MAX_POINTS_PER_READ, total - offset);
        ranges.push({area, startAddress: range.start + offset, count});
        offset += count;
      }
    }
  }
  return ranges;
};
