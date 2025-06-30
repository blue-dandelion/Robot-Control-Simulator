import { TextInput, TextInputProps, useColorScheme } from 'react-native'
import React from 'react'
import { Colors } from '../constants/colors'
import { Styles } from '../constants/styles'
import { AlignSelfType } from '../constants/deps'

interface Props extends TextInputProps{
  style?: any;
  value?: string;
  alignSelf?: AlignSelfType;
  onChangeText?: (text: string) => void;
};

const ThemedTextInput: React.FC<Props> = ({ style, value, alignSelf, onChangeText, ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    return (
        <TextInput
            style={[{ color: theme.text, borderWidth: 1, borderColor: theme.decoLine, alignSelf: alignSelf }, Styles.txtinput, style]}
            value={value}
            onChangeText={onChangeText}>
        </TextInput>
    )
}

export default ThemedTextInput