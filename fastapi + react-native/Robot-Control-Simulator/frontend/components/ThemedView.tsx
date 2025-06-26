import { View } from "react-native"
import { useSafeAreaInsets } from "react-native-safe-area-context";

const ThemedView = ({ style = {}, safe = false, ...props }) => {

    if (!safe) return (<View style={style} {...props} />);

    const insets = useSafeAreaInsets();
    return (
        <View style={[{ paddingTop: insets.top, paddingBottom: insets.bottom}, style]} {...props} />
    )
}

export default ThemedView