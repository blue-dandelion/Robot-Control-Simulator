import { View, useColorScheme, ViewProps, ViewStyle } from 'react-native'
import React from 'react'
import { Colors } from '../constants/colors'

type Direction = 'Horizontal' | 'Vertical';

interface DecoLineProps extends ViewProps {
    direction?: Direction;
    style?: ViewStyle;
}

const DecoLine: React.FC<DecoLineProps> = ({ direction, style, ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    const orientationStyle: ViewStyle = direction === 'Horizontal' ? { width: '100%', height: 2 } : { width: 1, height: '100%' };

    return (
        <View style={[{
            backgroundColor: theme.decoLine,
        }, orientationStyle, style]}>

        </View>
    )
}

export default DecoLine