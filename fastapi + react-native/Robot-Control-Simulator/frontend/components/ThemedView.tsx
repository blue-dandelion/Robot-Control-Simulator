import { useColorScheme, View } from "react-native"
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Colors } from "../constants/colors";

const ThemedView = ({ style = {}, safe = false, ...props }) => {
    const colorScheme = useColorScheme()
    const theme = colorScheme ? Colors[colorScheme] : Colors.light

    if (!safe)
        return (<View
            style={[{ backgroundColor: theme.background_bl }, style]}
            {...props}
        />);

    const insets = useSafeAreaInsets();
    return (
        <View
            style={[{ paddingTop: insets.top, paddingBottom: insets.bottom, backgroundColor: theme.background_bl }, style]}
            {...props}
        />
    )
}

export default ThemedView