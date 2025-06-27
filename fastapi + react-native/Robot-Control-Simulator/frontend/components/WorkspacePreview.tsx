import { View, Text, useColorScheme, ViewProps } from 'react-native'
import React, { forwardRef, useEffect, useImperativeHandle, useRef, useState } from 'react'
import { Colors } from '../constants/colors'
import { Direction, RotateTo } from '../constants/deps';

export interface WorkspacePreviewHandles {
    place(x: string, y: string, facing: Direction): void;
    move(): void;
    rotate(to: RotateTo): void;
    report(): void;
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

    const isInWorkspace = (x: number, y: number): boolean => {
        if (x >= 0 && x < rows && y >= 0 && y < cols) {
            return true;
        }
        else {
            return false;
        }
    }

    useEffect(() =>{
        facingRef.current = facing
    }, [facing])

    useImperativeHandle(ref, () => ({
        place(x: string, y: string, facing: Direction) {
            const newX = parseInt(x, 10);
            const newY = parseInt(y, 10);
            if (isInWorkspace(newX, newY)) {
                setPosX(newX);
                setPosY(newY);
                setFacing(facing);
            }
            else {

            }
        },
        move() {
            let newX = posX;
            let newY = posY;

            switch (facingRef.current) {
                case Direction.NORTH:
                    setPosY(preY => {
                        const newY = preY + 1;
                        if(isInWorkspace(posX, newY))
                            return newY
                        return preY
                    })
                    break;
                case Direction.EAST:
                    setPosX(preX => {
                        const newX = preX + 1;
                        if(isInWorkspace(newX, posY))
                            return newX
                        return preX
                    })
                    break;
                case Direction.SOUTH:
                    setPosY(preY => {
                        const newY = preY - 1;
                        if(isInWorkspace(posX, newY))
                            return newY
                        return preY
                    })
                    break;
                case Direction.WEST:
                    setPosX(preX => {
                        const newX = preX - 1;
                        if(isInWorkspace(newX, posY))
                            return newX
                        return preX
                    })
                    break;
                default:
                    break;
            }
        },
        rotate(to: RotateTo) {
            switch(to){
                case RotateTo.LEFT:
                    setFacing(prev => (prev + 3) % 4)
                    break;
                case RotateTo.RIGHT:
                    setFacing(prev => (prev + 1) % 4)
                    break;
                default:
                    break;
            }
        },
        report() {

        },
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