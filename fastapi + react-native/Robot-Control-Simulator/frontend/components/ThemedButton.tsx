import { Pressable } from 'react-native'
import React from 'react'
import { Styles } from '../constants/styles'

const ThemedButton = ({ style = {}, ...props }) => {
    return (
        <Pressable
            style={({ pressed }) => [Styles.btn, style]}
            {...props}
        />
    )
}

export default ThemedButton