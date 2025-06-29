import { View, Text, useColorScheme, ViewProps } from 'react-native'
import React, { forwardRef, useEffect, useImperativeHandle, useRef, useState } from 'react'
import { Colors } from '../constants/colors'
import { Direction, RotateTo } from '../constants/deps';

export interface WorkspacePreviewHandles {
    place(x: number, y: number, facing: Direction): void;
    move(x: number, y: number): void;
    rotate(dir: Direction): void;
}

export interface Props extends ViewProps {
}

const WorkspacePreview = forwardRef<WorkspacePreviewHandles, Props>(({ style, children, ...rest }, ref) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    const [unitSize, setUnitSize] = useState<number>(20);
    const [rows, setRows] = useState<number>(5);
    const [cols, setCols] = useState<number>(5);
    const [posX, setPosX] = useState<number>(0);
    const [posY, setPosY] = useState<number>(0);
    const [facing, setFacing] = useState<Direction>(Direction.NORTH);

    const facingRef = useRef(facing);

    useEffect(() =>{
        facingRef.current = facing
    }, [facing])

    useImperativeHandle(ref, () => ({
        place(x: number, y: number, facing: Direction) {
            setPosX(x);
            setPosY(y);
            setFacing(facing);
        },
        move(x: number, y: number) {
            setPosX(x)
            setPosY(y)
        },
        rotate(dir: Direction) {
            setFacing(dir)
        }
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
                bottom: posY * unitSize,
                left: posX * unitSize,
                transform: [{ rotate: `${facing * 90}deg` }],
                borderTopLeftRadius: unitSize / 2,
                borderTopRightRadius: unitSize / 2,
                width: unitSize, height: unitSize, backgroundColor: '#c22'
            }}>

            </View>
        </View >
    )
});

export default WorkspacePreview