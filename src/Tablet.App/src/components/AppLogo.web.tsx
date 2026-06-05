import React from 'react';
import {Image} from 'react-native';
import type {ImageStyle, StyleProp} from 'react-native';
import logoUrl from '../../Logo.png';

interface Props {
  style: StyleProp<ImageStyle>;
}

export const AppLogo = ({style}: Props) => (
  <Image
    accessibilityLabel="Gear Line logo"
    resizeMode="contain"
    source={{uri: logoUrl}}
    style={style}
    testID="app-logo"
  />
);
