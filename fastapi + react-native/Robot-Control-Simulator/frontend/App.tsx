import { Styles } from './constants/styles';
import { Alert, Button, Pressable, ScrollView, Text, TextInput, useColorScheme, View } from 'react-native';
import ThemedView from './components/ThemedView';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { useState } from 'react';
import ThemedButton from './components/ThemedButton';
import { Colors } from './constants/colors';
import ThemedText from './components/ThemedText';
import DecoLine from './components/DecoLine';
import WorkspacePreview from './components/WorkspacePreview';

export default function App() {
  const colorScheme = useColorScheme()
  const theme = colorScheme ? Colors[colorScheme] : Colors.light

  const [code, setCode] = useState('')
  const [errors, setErrors] = useState('')
  const [outputs, setOutputs] = useState('')

  const runCode = async () => {
    try {
      // machine's LAN IP
      const BASE_URL = 'http://192.168.1.206:8000'

      const result = await fetch(`${BASE_URL}/process`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ text: code })
        }
      );

      if (!result.ok) throw new Error(`HTTP ${result.status}`);
      const json = await result.json();
      setErrors(json.errors)
      setOutputs(json.outputs)
    }
    catch (err) {
      if (err instanceof Error)
        Alert.alert('Error', err.message)
    }
  }

  return (
    <SafeAreaProvider>
      <ThemedView safe={true} style={{ flex: 1, flexDirection: 'column' }}>
        <View style={{ flexDirection: 'row', justifyContent: 'center', width: '100%', height: 'auto', padding: 10, backgroundColor: theme.background_tl }}>
          <ThemedButton text='Run' onPress={runCode} />
        </View>
        <DecoLine direction='Horizontal' />

        <ScrollView>
          {/* Preview */}
          <View style={{ backgroundColor: '#000', height: 400, alignItems: 'center', justifyContent: 'center' }}>
            <WorkspacePreview />
          </View>

          {/* IDE */}
          <View style={{ alignItems: 'flex-start', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Code</ThemedText>
            <ScrollView style={{ width: '100%', height: 200, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 200, marginHorizontal: 10, fontSize: 16, color: theme.text }} value={code} multiline={true} onChangeText={setCode} />
            </ScrollView>
          </View>

          {/* Error */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Error</ThemedText>
            <ScrollView style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 100, marginHorizontal: 10, fontSize: 16, color: theme.text }} editable={false} value={errors} multiline={true} onChangeText={setErrors} />
            </ScrollView>
          </View>

          {/* Output */}
          <View style={{ alignItems: 'flex-start', width: '100%', height: 'auto', paddingTop: 10 }}>
            <ThemedText title={true} style={{ marginLeft: 10, marginBottom: 5 }}>Output</ThemedText>
            <ScrollView style={{ width: '100%', height: 100, backgroundColor: theme.background_tl }}>
              <TextInput style={{ width: '100%', minHeight: 100, marginHorizontal: 10, fontSize: 16, color: theme.text }} editable={false} value={outputs} multiline={true} onChangeText={setOutputs} />
            </ScrollView>
          </View>

          <View style={{ height: 500 }} />
        </ScrollView>

      </ThemedView >

    </SafeAreaProvider >

  );
}

