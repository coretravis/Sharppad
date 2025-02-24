export function initializeMonaco(elementId, initialCode, dotnetRef, theme, serverUrl, formatCodeElementId) {
    require.config({ paths: { 'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.33.0/min/vs' } });
    require(['vs/editor/editor.main'], function () {
        window.monacoEditor = monaco.editor.create(document.getElementById(elementId), {
            value: initialCode,
            language: 'csharp',
            theme: theme != null ? theme : 'vs-dark',
            automaticLayout: true
        });

        // Add onChange event listener
        window.monacoEditor.onDidChangeModelContent((event) => {
            const newContent = window.monacoEditor.getValue();
            // Call the Blazor method
            dotnetRef.invokeMethodAsync('OnEditorContentChanged', newContent);
        });

        // Reference the active Monaco editor instance
        const editor = monaco.editor.getModels()[0];

        /**
         * 1️ Auto-Completion (IntelliSense)
         */
        monaco.languages.registerCompletionItemProvider("csharp", {
            provideCompletionItems: async (model, position) => {
                const code = model.getValue();
                const cursorOffset = model.getOffsetAt(position);

                const response = await fetch(`${serverUrl}/api/roslyn/autocomplete`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ code: code, position: cursorOffset }),
                });

                const suggestions = await response.json();

                return {
                    suggestions: suggestions.map(suggestion => ({
                        label: suggestion,
                        kind: monaco.languages.CompletionItemKind.Method,
                        insertText: suggestion
                    }))
                };
            }
        });

        /**
         * 2️ Syntax Error Checking (Diagnostics)
         */
        async function checkDiagnostics() {
            const code = editor.getValue();

            const response = await fetch(`${serverUrl}/api/roslyn/diagnostics`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ code: code })
            });

            const diagnostics = await response.json();
            const markers = diagnostics.map(error => ({
                severity: monaco.MarkerSeverity.Error,
                message: error.message,
                startLineNumber: error.startLine,
                startColumn: error.startColumn,
                endLineNumber: error.endLine,
                endColumn: error.endColumn
            }));

            monaco.editor.setModelMarkers(editor, "csharp", markers);
        }

        // Run diagnostics every time the user stops typing
        editor.onDidChangeContent(() => {
            setTimeout(checkDiagnostics, 1000); // Delay to avoid excessive API calls
        });

        /**
         * 3️ Code Formatting
         */
        async function formatCode() {
            const code = editor.getValue();

            const response = await fetch(`${serverUrl}/api/roslyn/format`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ code: code })
            });

            const formattedCode = await response.text();
            editor.setValue(formattedCode);
        }

        try {
            document.getElementById(formatCodeElementId).addEventListener("click", formatCode);
        } catch (e) {
            // inform user code formatting was not successful
        }
        

        /**
         * 4️ Go-To Definition
         */
        monaco.languages.registerDefinitionProvider("csharp", {
            provideDefinition: async (model, position) => {
                const code = model.getValue();
                const cursorOffset = model.getOffsetAt(position);

                const response = await fetch(`${serverUrl}/api/roslyn/definition`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ code: code, position: cursorOffset })
                });

                const definition = await response.text();

                // todo:  enhance this to show a tooltip 
                return [];
            }
        });


    });
}


export function initializeEmbedMonaco(elementId, initialCode, dotnetRef, theme) {
    require.config({ paths: { 'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.33.0/min/vs' } });
    require(['vs/editor/editor.main'], function () {
        window.monacoEditor = monaco.editor.create(document.getElementById(elementId), {
            value: initialCode,
            language: 'csharp',
            theme: theme != null ? theme : 'vs-dark',
            automaticLayout: true
        });

        // Add onChange event listener
        window.monacoEditor.onDidChangeModelContent((event) => {
            const newContent = window.monacoEditor.getValue();
            // Call the Blazor method
            dotnetRef.invokeMethodAsync('OnEditorContentChanged', newContent);
        });

        // Reference the active Monaco editor instance
        const editor = monaco.editor.getModels()[0];


        // Run diagnostics every time the user stops typing
        editor.onDidChangeContent(() => {
            setTimeout(checkDiagnostics, 1000); // Delay to avoid excessive API calls
        });

    });
}

export function getEditorContent(elementId) {
    if (window.monacoEditor) {
        return window.monacoEditor.getValue();
    }
    return "";
}

export function setEditorContent(elementId, code) {
    if (window.monacoEditor) {
        window.monacoEditor.setValue(code);
    }
}

export function scrollToBottom(element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}


export function downloadFile(filename, contentType, content) {
    // Create a blob with the content and specified type
    const blob = new Blob([content], { type: contentType });

    // Create a URL for the blob
    const url = window.URL.createObjectURL(blob);

    // Create a temporary link element
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;

    // Append the link to the document
    document.body.appendChild(link);

    // Trigger the download
    link.click();

    // Clean up
    window.URL.revokeObjectURL(url);
    document.body.removeChild(link);
};

// Helper function to convert data to CSV format
export function convertToCSV(data) {
    try {
        // Parse the data if it's a JSON string
        const jsonData = typeof data === 'string' ? JSON.parse(data) : data;

        // Handle array of objects
        if (Array.isArray(jsonData)) {
            if (jsonData.length === 0) return '';

            // Get headers from the first object
            const headers = Object.keys(jsonData[0]);

            // Create CSV rows
            const csvRows = [
                // Header row
                headers.join(','),
                // Data rows
                ...jsonData.map(row =>
                    headers.map(header =>
                        JSON.stringify(row[header] ?? '')
                    ).join(',')
                )
            ];

            return csvRows.join('\n');
        }

        // Handle single object
        const headers = Object.keys(jsonData);
        const values = headers.map(header =>
            JSON.stringify(jsonData[header] ?? '')
        );

        return headers.join(',') + '\n' + values.join(',');
    } catch (error) {
        console.error('Error converting to CSV:', error);
        return data.toString();
    }
};