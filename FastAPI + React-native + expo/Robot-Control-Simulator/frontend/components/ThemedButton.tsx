import { Text, Pressable, useColorScheme, ButtonProps, ViewStyle, TextStyle, GestureResponderEvent, PressableProps } from 'react-native'
import React from 'react'
import { Styles } from '../constants/styles'
import { Colors } from '../constants/colors'

interface Props extends PressableProps {
    style?: TextStyle;
    text?: string;
    disabled?: boolean;
    children?: any;
    onPress?: (e: GestureResponderEvent) => void;
};

const ThemedButton: React.FC<Props> = ({ style, text, disabled, children, onPress, ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    return (
        <Pressable
            style={({ pressed }) => [Styles.btn, { backgroundColor: theme.background_ui }, pressed && { opacity: 0.8 }, disabled && { opacity: 0.5 }, style]}
            onPress={onPress}
            disabled={disabled}
            {...props}
        >
            {children == <></> ? (
                children
            ) : (
                <Text style={[Styles.txt_ui, { color: theme.text }]}>
                    {text}
                </Text>
            )}
        </Pressable>
    );
}

export default ThemedButton