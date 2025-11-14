export interface PrinterConfiguration {
  printerType: 'A4' | 'POS' | 'Thermal' | 'Label';
  paperFormat: string; // A4, Letter, Legal, etc.
  paperWidth: number; // mm
  paperHeight: number; // mm (0 = continuous)
  margins: PrinterMargins;
  fontSettings: PrinterFontSettings;
  posSettings?: PosPrinterSettings;
  printInColor: boolean;
  printBackground: boolean;
  orientation: 'Portrait' | 'Landscape';
  scale: number;
}

export interface PrinterMargins {
  top: string;
  right: string;
  bottom: string;
  left: string;
}

export interface PrinterFontSettings {
  fontFamily: string;
  baseFontSize: number;
  headerFontSize: number;
  footerFontSize: number;
  tableFontSize: number;
}

export interface PosPrinterSettings {
  encoding: string;
  copies: number;
  autoCut: boolean;
  openCashDrawer: boolean;
  printDensity: number;
  printSpeed: number;
  useEscPos: boolean;
  connectionType: 'USB' | 'Network' | 'Serial' | 'Bluetooth';
  connectionString?: string | null;
}

// Default printer configurations
export const defaultA4PrinterConfig: PrinterConfiguration = {
  printerType: 'A4',
  paperFormat: 'A4',
  paperWidth: 210, // A4 width in mm
  paperHeight: 297, // A4 height in mm
  margins: {
    top: '20mm',
    right: '20mm',
    bottom: '20mm',
    left: '20mm'
  },
  fontSettings: {
    fontFamily: 'Arial',
    baseFontSize: 12,
    headerFontSize: 16,
    footerFontSize: 10,
    tableFontSize: 11
  },
  printInColor: true,
  printBackground: true,
  orientation: 'Portrait',
  scale: 1.0
};

export const defaultPosPrinterConfig: PrinterConfiguration = {
  printerType: 'POS',
  paperFormat: 'Thermal',
  paperWidth: 80, // 80mm thermal paper
  paperHeight: 0, // Continuous roll
  margins: {
    top: '5mm',
    right: '5mm',
    bottom: '5mm',
    left: '5mm'
  },
  fontSettings: {
    fontFamily: 'Courier',
    baseFontSize: 10,
    headerFontSize: 12,
    footerFontSize: 8,
    tableFontSize: 9
  },
  posSettings: {
    encoding: 'UTF-8',
    copies: 1,
    autoCut: true,
    openCashDrawer: false,
    printDensity: 8,
    printSpeed: 3,
    useEscPos: true,
    connectionType: 'USB',
    connectionString: null
  },
  printInColor: false,
  printBackground: false,
  orientation: 'Portrait',
  scale: 1.0
};

// Helper function to parse printer configuration from JSON string
export function parsePrinterConfiguration(jsonString: string | null | undefined): PrinterConfiguration {
  if (!jsonString) {
    return defaultA4PrinterConfig;
  }

  try {
    const parsed = JSON.parse(jsonString);
    // Merge with defaults to ensure all fields are present
    return {
      ...defaultA4PrinterConfig,
      ...parsed,
      margins: { ...defaultA4PrinterConfig.margins, ...parsed.margins },
      fontSettings: { ...defaultA4PrinterConfig.fontSettings, ...parsed.fontSettings },
      posSettings: parsed.posSettings ? {
        ...defaultPosPrinterConfig.posSettings,
        ...parsed.posSettings
      } : undefined
    };
  } catch (error) {
    console.error('Error parsing printer configuration:', error);
    return defaultA4PrinterConfig;
  }
}

// Helper function to serialize printer configuration to JSON string
export function serializePrinterConfiguration(config: PrinterConfiguration): string {
  return JSON.stringify(config);
}

