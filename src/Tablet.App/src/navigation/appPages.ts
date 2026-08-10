export type AppPage = 'auto' | 'manual' | 'io' | 'history' | 'settings';

export interface AppPageDefinition {
  key: AppPage;
  label: string;
}

export const appPages: AppPageDefinition[] = [
  {
    key: 'auto',
    label: 'Tự động',
  },
  {
    key: 'manual',
    label: 'Bằng tay',
  },
  {
    key: 'io',
    label: 'Giám sát IO',
  },
  {
    key: 'history',
    label: 'Lịch sử',
  },
  {
    key: 'settings',
    label: 'Cài đặt',
  },
];
