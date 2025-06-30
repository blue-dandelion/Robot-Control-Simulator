import { TextInput, TextInputProps, TextStyle, useColorScheme } from 'react-native'
import React from 'react'
import { Colors } from '../constants/colors'
import { Styles } from '../constants/styles'

interface Props extends TextInputProps {
    style?: TextStyle;
    type?: 'text' | 'int' | 'digit'
    placeholder?: string;
    multiline?: boolean;
    value?: string;
    onChangeText?: (text: string) => void;
};

const ThemedTextInput: React.FC<Props> = ({ style, type, placeholder, multiline, value, onChangeText, ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    return (
        <TextInput
            style={[{ color: theme.text, borderWidth: 1, borderColor: theme.decoLine, padding: 2 }, Styles.txtinput, style]}
            value={value}
            placeholder={placeholder}
            multiline={multiline}
            onChangeText={onChangeText}>
        </TextInput>
    )
}

export default ThemedTextInput