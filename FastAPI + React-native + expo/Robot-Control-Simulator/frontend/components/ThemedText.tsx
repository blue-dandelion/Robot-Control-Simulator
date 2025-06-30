import { Text, TextProps, TextStyle, useColorScheme } from 'react-native'
import React from 'react'
import { Styles } from '../constants/styles'
import { Colors } from '../constants/colors'

interface Props extends TextProps {
    style?: TextStyle;
    value?: string;
    title?: boolean;
    children?: string;
};

const ThemedText: React.FC<Props> = ({ style, children, title = false, ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    return (
        <Text
            style={[{ color: theme.text }, title ? Styles.title : Styles.txt, style]}
            {...props}>
            {children}
        </Text>
    )
}

export default ThemedText