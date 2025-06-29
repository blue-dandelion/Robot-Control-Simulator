import { Text, Pressable, useColorScheme } from 'react-native'
import React from 'react'
import { Styles } from '../constants/styles'
import { Colors } from '../constants/colors'



const ThemedButton = ({ style = {}, children = <></>, onPress = () => { }, text = "", ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    return (
        <Pressable
            style={({ pressed }) => [Styles.btn, { backgroundColor: theme.background_ui }, pressed && { opacity: 0.8 }, style]}
            onPress={onPress}
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