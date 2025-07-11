import { KeyboardTypeOptions, TextInput, TextInputProps, TextStyle, useColorScheme } from 'react-native'
import React from 'react'
import { Colors } from '../constants/colors'
import { Styles } from '../constants/styles'

interface Props extends TextInputProps {
    style?: TextStyle;
    type?: 'text' | 'int' | 'digit' | 'url'
    placeholder?: string;
    multiline?: boolean;
    editable?: boolean;
    value?: string;
    onChangeText?: (text: string) => void;
};

const keyboardTypeMap: Record<string, KeyboardTypeOptions> = {
    int: 'number-pad',
    digit: 'decimal-pad',
    url: 'numbers-and-punctuation',
    text: 'default',
}

const ThemedTextInput: React.FC<Props> = ({ style, type, placeholder, multiline, editable = true, value, onChangeText, ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    return (
        <TextInput
            style={[{ color: theme.text, borderWidth: editable ? 1 : 0, borderColor: theme.decoLine, padding: 2 }, Styles.txtinput, style]}
            value={value}
            keyboardType={keyboardTypeMap[type ? type : 'text']}
            placeholder={placeholder}
            placeholderTextColor={theme.placeholder}
            editable={editable}
            multiline={multiline}
            onChangeText={onChangeText}>
        </TextInput>
    )
}

export default ThemedTextInput