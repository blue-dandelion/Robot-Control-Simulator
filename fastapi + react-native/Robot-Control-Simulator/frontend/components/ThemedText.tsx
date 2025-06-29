import { Text, TextProps, useColorScheme } from 'react-native'
import React from 'react'
import { Styles } from '../constants/styles'
import { Colors } from '../constants/colors'
import { AlignSelfType } from '../constants/deps';

interface Props extends TextProps {
    style?: any;
    value?: string;
    title: boolean;
    alignSelf?: AlignSelfType;
    children?: string;
};

const ThemedText: React.FC<Props> = ({ style, children, title = false, alignSelf, ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    return (
        <Text
            style={[{ color: theme.text, alignSelf: alignSelf }, title ? Styles.title : Styles.txt, style]}
            {...props}>
            {children}
        </Text>
    )
}

export default ThemedText