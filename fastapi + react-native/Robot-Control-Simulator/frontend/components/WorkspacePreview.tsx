import { View, Text, useColorScheme, ViewProps } from 'react-native'
import React, { forwardRef, useImperativeHandle, useState } from 'react'
import { Colors } from '../constants/colors'

enum Direction { 'NORTH', 'EAST', 'SOUTH', 'WEST' }

export interface Handles {
    place(x: string, y: string, facing: Direction): void;
    move(): void;
    rotate(): void;
    report(): void;
}

interface Props extends ViewProps {
    initialX?: number;
    initialY?: number;
}

const WorkspacePreview = forwardRef<Handles, Props>(({ initialX = 0, initialY = 0, style, children, ...rest }, ref) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    const [unitSize, setUnitSize] = useState<number>(20);
    const [rows, setRows] = useState<number>(5);
    const [cols, setCols] = useState<number>(5);

    useImperativeHandle(ref, () => ({
        place(x: string, y: string, facing: Direction) { },
        move() { },
        rotate() { },
        report() { },
    }), []);

    const reload = (width: string, height: string) => {
        // Resize
        const w = parseInt(width, 10);
        const h = parseInt(height, 10);
        if (!isNaN(w) && w > 0) setCols(w);
        if (!isNaN(h) && h > 0) setRows(h);
    }

    return (
        <View style={{ position: 'relative' }}>
            {/*Workspace*/}
            <View style={{
                height: unitSize * rows, width: unitSize * cols
            }}>
                {
                    Array.from({ length: rows }).map((_, rowIndex) => (
                        <View key={rowIndex} style={{ flexDirection: 'row', height: unitSize }}>
                            {Array.from({ length: cols }).map((_, colIndex) => (
                                <View
                                    key={colIndex}
                                    style={[
                                        {
                                            width: `${100 / cols}%`,
                                            height: "100%",
                                            borderWidth: 1,
                                            borderColor: theme.decoLine,
                                            opacity: 0.5
                                        },
                                    ]}
                                />
                            ))}
                        </View>
                    ))
                }
            </View>

            {/*Robot*/}
            <View style={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                borderTopLeftRadius: unitSize / 2,
                borderTopRightRadius: unitSize / 2,
                width: unitSize, height: unitSize, backgroundColor: '#c22'
            }}>

            </View>
        </View >
    )
});

export default WorkspacePreview