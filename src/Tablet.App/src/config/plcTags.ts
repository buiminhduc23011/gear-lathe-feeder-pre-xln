import type {PlcTag} from '../types/plc';
import {createIoDisplayAddresses, defaultIoTagName, plcAddressForIoDisplayAddress} from './io/addressing';

const bit = (
  name: string,
  address: string,
  description: string,
  writable = false,
): PlcTag => ({name, address, dataType: 'Bool', description, writable});

const word = (
  name: string,
  address: string,
  dataType: PlcTag['dataType'],
  description: string,
  writable = false,
  length?: number,
): PlcTag => ({name, address, dataType, description, writable, length});

const ioGridTags: PlcTag[] = [
  ...createIoDisplayAddresses('X').map(displayAddress =>
    bit(defaultIoTagName('input', displayAddress), plcAddressForIoDisplayAddress(displayAddress), `Input ${displayAddress}`),
  ),
  ...createIoDisplayAddresses('Y').map(displayAddress =>
    bit(defaultIoTagName('output', displayAddress), plcAddressForIoDisplayAddress(displayAddress), `Output ${displayAddress}`),
  ),
];

export const plcTags: PlcTag[] = [
  ...ioGridTags,

  bit('manual.move_x_forward', 'M2000', 'Tiến trục X', true),
  bit('manual.move_x_backward', 'M2001', 'Lùi trục X', true),
  bit('manual.home_x', 'M2002', 'Về góc trục X', true),
  bit('manual.move_x_to_point', 'M2003', 'Chạy điểm vị trí trục X', true),
  word('manual.manual_speed_x', 'D5150', 'Float', 'Tốc độ Manual trục X (mm/s)', true),
  word('manual.move_point_x', 'D5152', 'Float', 'Vị trí chạy điểm trục X (mm)', true),
  word('manual.current_position_x', 'D5154', 'Float', 'Vị trí hiện tại trục X (mm)'),
  bit('manual.is_homing_x', 'M2040', 'Đang về Home X'),
  bit('manual.is_homed_x', 'M2041', 'Đã về Home X'),

  bit('manual.move_y_left', 'M2004', 'Trái trục Y', true),
  bit('manual.move_y_right', 'M2005', 'Phải trục Y', true),
  bit('manual.home_y', 'M2006', 'Về góc trục Y', true),
  bit('manual.move_y_to_point', 'M2007', 'Chạy điểm vị trí trục Y', true),
  word('manual.manual_speed_y', 'D5156', 'Float', 'Tốc độ Manual trục Y (mm/s)', true),
  word('manual.move_point_y', 'D5158', 'Float', 'Vị trí chạy điểm trục Y (mm)', true),
  word('manual.current_position_y', 'D5160', 'Float', 'Vị trí hiện tại trục Y (mm)'),
  bit('manual.is_homing_y', 'M2042', 'Đang về Home Y'),
  bit('manual.is_homed_y', 'M2043', 'Đã về Home Y'),

  bit('manual.move_z_up', 'M2008', 'Lên trục Z', true),
  bit('manual.move_z_down', 'M2009', 'Xuống trục Z', true),
  bit('manual.home_z', 'M2010', 'Về góc trục Z', true),
  bit('manual.move_z_to_point', 'M2011', 'Chạy điểm vị trí trục Z', true),
  word('manual.manual_speed_z', 'D5162', 'Float', 'Tốc độ Manual trục Z (mm/s)', true),
  word('manual.move_point_z', 'D5164', 'Float', 'Vị trí chạy điểm trục Z (mm)', true),
  word('manual.current_position_z', 'D5166', 'Float', 'Vị trí hiện tại trục Z (mm)'),
  bit('manual.is_homing_z', 'M2044', 'Đang về Home Z'),
  bit('manual.is_homed_z', 'M2045', 'Đã về Home Z'),

  bit('manual.change_tool_to_0', 'M2012', 'Xilanh đổi tay tool 0 độ', true),
  bit('manual.change_tool_to_180', 'M2013', 'Xilanh đổi tay tool 180 độ', true),
  bit('manual.clamp_small_part_in', 'M2014', 'Xilanh part nhỏ kẹp', true),
  bit('manual.clamp_small_part_out', 'M2015', 'Xilanh part nhỏ mở', true),
  bit('manual.clamp_large_part_in', 'M2016', 'Xilanh part lớn kẹp', true),
  bit('manual.clamp_large_part_out', 'M2017', 'Xilanh part lớn mở', true),
  bit('manual.rotate_tool_to_0', 'M2018', 'Xilanh xoay tay tool điểm 0', true),
  bit('manual.rotate_tool_to_90', 'M2019', 'Xilanh xoay tay tool quay 90', true),
  bit('manual.clamp_cart_1', 'M2020', 'Xilanh kẹp xe hàng 1', true),
  bit('manual.unclamp_cart_1', 'M2021', 'Xilanh mở xe hàng 1', true),
  bit('manual.clamp_cart_2', 'M2022', 'Xilanh kẹp xe hàng 2', true),
  bit('manual.unclamp_cart_2', 'M2023', 'Xilanh mở xe hàng 2', true),
  bit('manual.home_all', 'M2024', 'Home ALL', true),
  bit('manual.home_cart_1_clamp', 'M2025', 'Home kẹp xe 1', true),
  bit('manual.home_cart_2_clamp', 'M2026', 'Home kẹp xe 2', true),
  bit('manual.home_rotate_cylinder', 'M2027', 'Home xilanh xoay', true),
  bit('manual.home_tool_change_cylinder', 'M2028', 'Home xilanh đảo tool', true),
  bit('manual.home_tool_clamp_cylinder', 'M2029', 'Home xilanh kẹp tay tool', true),

  bit('manual.cart_1_opened_signal', 'M2046', 'Tín hiệu đã mở kẹp xe hàng 1'),
  bit('manual.cart_1_closed_signal', 'M2047', 'Tín hiệu đã kẹp xe hàng 1'),
  bit('manual.cart_2_opened_signal', 'M2048', 'Tín hiệu đã mở kẹp xe hàng 2'),
  bit('manual.cart_2_closed_signal', 'M2049', 'Tín hiệu đã kẹp xe hàng 2'),
  bit('manual.small_part_closed_signal', 'M2050', 'Tín hiệu đã kẹp part nhỏ'),
  bit('manual.small_part_opened_signal', 'M2051', 'Tín hiệu đã mở kẹp part nhỏ'),
  bit('manual.large_part_closed_signal', 'M2052', 'Tín hiệu đã kẹp part lớn'),
  bit('manual.large_part_opened_signal', 'M2053', 'Tín hiệu đã mở kẹp part lớn'),
  bit('manual.rotated_to_0_signal', 'M2054', 'Tín hiệu đã quay về 0'),
  bit('manual.rotated_to_90_signal', 'M2055', 'Tín hiệu đã quay về 90'),
  bit('manual.changed_tool_to_0_signal', 'M2056', 'Tín hiệu đã đổi tay tool 0'),
  bit('manual.changed_tool_to_180_signal', 'M2057', 'Tín hiệu đã đổi tay tool 180'),
  bit('manual.home_cart_1_clamp_done', 'M2058', 'Đã về home kẹp xe hàng 1'),
  bit('manual.home_cart_2_clamp_done', 'M2059', 'Đã về home kẹp xe hàng 2'),
  bit('manual.home_rotate_cylinder_done', 'M2060', 'Đã về home xilanh xoay'),
  bit('manual.home_tool_clamp_done', 'M2061', 'Đã về home kẹp tay tool'),
  bit('manual.home_tool_change_done', 'M2062', 'Đã về home đảo tay tool'),

  bit('alarm.estop', 'D5142.0', 'E_EMG'),
  bit('alarm.safety_sensor', 'D5142.15', 'E_SAFETY_SENSOR'),
  bit('alarm.x_limit_negative', 'D5143.0', 'E_LIMIT X -'),
  bit('alarm.x_limit_positive', 'D5143.1', 'E_LIMIT X +'),
  bit('alarm.y_limit_negative', 'D5143.2', 'E_LIMIT Y -'),
  bit('alarm.y_limit_positive', 'D5143.3', 'E_LIMIT Y +'),
  bit('alarm.z_limit_negative', 'D5143.4', 'E_LIMIT Z -'),
  bit('alarm.z_limit_positive', 'D5143.5', 'E_LIMIT Z +'),
  bit('alarm.air_pressure_lost', 'D5143.15', 'E_PRESSURE_AIR'),
  word('data.max_speed_x', 'D21070', 'Float', 'Giới hạn tốc độ trục X'),
  word('data.max_speed_y', 'D21072', 'Float', 'Giới hạn tốc độ trục Y'),
  word('data.max_speed_z', 'D21074', 'Float', 'Giới hạn tốc độ trục Z'),
  word('data.limit_x_positive', 'D21076', 'Float', 'Limit trục X+'),
  word('data.limit_x_negative', 'D21078', 'Float', 'Limit trục X-'),
  word('data.limit_y_positive', 'D21080', 'Float', 'Limit trục Y+'),
  word('data.limit_y_negative', 'D21082', 'Float', 'Limit trục Y-'),
  word('data.limit_z_positive', 'D21084', 'Float', 'Limit trục Z+'),
  word('data.limit_z_negative', 'D21086', 'Float', 'Limit trục Z-'),
];

export const plcTagByName = new Map(plcTags.map(tag => [tag.name, tag]));
