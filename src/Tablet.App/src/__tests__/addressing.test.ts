import {createBinding} from '@sti/delta-plc';
import type {PlcTag} from '../types/plc';

const tag = (address: string, dataType: PlcTag['dataType'] = 'Bool'): PlcTag => ({
  name: `tag.${address}`,
  address,
  dataType,
  description: address,
  length: dataType === 'String' ? 30 : undefined,
});

describe('PLC address parser', () => {
  it('parses M coils', () => {
    expect(createBinding(tag('M2000'))).toMatchObject({area: 'coil', startAddress: 2000, span: 1});
  });

  it('maps M coils through Delta DVP offsets when requested', () => {
    expect(createBinding(tag('M2000'), 'TcpDVP')).toMatchObject({area: 'coil', startAddress: 0x0800 + 2000, span: 1});
  });

  it('parses D words', () => {
    expect(createBinding(tag('D5150', 'Float'))).toMatchObject({area: 'holding', startAddress: 5150, span: 2});
  });

  it('maps DVP D words through the Delta register base', () => {
    expect(createBinding(tag('D150', 'Float'), 'TcpDVP')).toMatchObject({area: 'holding', startAddress: 0x1000 + 150, span: 2});
  });

  it('parses D bit addresses', () => {
    expect(createBinding(tag('D5032.0'))).toMatchObject({area: 'holding', startAddress: 5032, span: 1, bitIndex: 0});
  });

  it('parses X inputs as discrete inputs', () => {
    expect(createBinding(tag('X9'))).toMatchObject({area: 'discrete', startAddress: 0x6000 + 9, span: 1});
  });

  it('rejects unsupported bit ranges', () => {
    expect(() => createBinding(tag('D5032.16'))).toThrow(/outside the supported D-word bit range/);
  });

  it('rejects manual config D addresses that exceed the Delta DVP range', () => {
    expect(() => createBinding(tag('D21070', 'Float'), 'TcpDVP')).toThrow(/D0-D4999/);
  });
});
