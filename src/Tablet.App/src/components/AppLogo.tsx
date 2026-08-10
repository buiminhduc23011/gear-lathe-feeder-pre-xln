import React from 'react';
import {Image} from 'react-native';
import type {ImageStyle, StyleProp} from 'react-native';

interface Props {
  style: StyleProp<ImageStyle>;
}

export const AppLogo = ({style}: Props) => (
  <Image
    accessibilityLabel="Gear Line logo"
    resizeMode="contain"
    source={require('../../Logo.png')}
    style={style}
    testID="app-logo"
  />
);
