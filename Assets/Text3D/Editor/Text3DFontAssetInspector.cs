using System;
using System.IO;
using Autodesk.Fbx;
using Text3D.Scripts;
using texttools;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
using Path = System.IO.Path;


namespace Text3D.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Text3DFont))]
    public class AssetInspector : UnityEditor.Editor
    {
        private static readonly Text3DCharacterRange[] Ranges =
        {
            new(0x0000, 0x001f, Text3DCharacterRangeType.ControlCharacter),
            new(0x0020, 0x007f, Text3DCharacterRangeType.BasicLatin),
            new(48, 57, Text3DCharacterRangeType.ZeroToNine),
            new(0x0080, 0x00ff, Text3DCharacterRangeType.Latin1Supplement),
            new(0x0100, 0x017f, Text3DCharacterRangeType.LatinExtendedA),
            new(0x0180, 0x024f, Text3DCharacterRangeType.LatinExtendedB),
            // new(0x0250, 0x02af, Text3DCharacterRangeType.IPAExtensions),
            // new(0x02b0, 0x02ff, Text3DCharacterRangeType.SpacingModifierLetters),
            // new(0x0300, 0x036f, Text3DCharacterRangeType.CombiningDiacriticalMarks),
            // new(0x0370, 0x03ff, Text3DCharacterRangeType.GreekAndCoptic),
            // new(0x0400, 0x04ff, Text3DCharacterRangeType.Cyrillic),
            // new(0x0500, 0x052f, Text3DCharacterRangeType.CyrillicSupplementary),
            // new(0x0530, 0x058f, Text3DCharacterRangeType.Armenian),
            // new(0x0590, 0x05ff, Text3DCharacterRangeType.Hebrew),
            // new(0x0600, 0x06ff, Text3DCharacterRangeType.Arabic),
            // new(0x0700, 0x074f, Text3DCharacterRangeType.Syriac),
            // new(0x0750, 0x077f, Text3DCharacterRangeType.ArabicSupplement),
            // new(0x0780, 0x07bf, Text3DCharacterRangeType.Thaana),
            // new(0x07c0, 0x07ff, Text3DCharacterRangeType.NKo),
            // new(0x0800, 0x083f, Text3DCharacterRangeType.Samaritan),
            // new(0x0840, 0x085f, Text3DCharacterRangeType.Mandaic),
            // new(0x08a0, 0x08ff, Text3DCharacterRangeType.ArabicExtendedA),
            // new(0x0900, 0x097f, Text3DCharacterRangeType.Devanagari),
            // new(0x0980, 0x09ff, Text3DCharacterRangeType.Bengali),
            // new(0x0a00, 0x0a7f, Text3DCharacterRangeType.Gurmukhi),
            // new(0x0a80, 0x0aff, Text3DCharacterRangeType.Gujarati),
            // new(0x0b00, 0x0b7f, Text3DCharacterRangeType.Oriya),
            // new(0x0b80, 0x0bff, Text3DCharacterRangeType.Tamil),
            // new(0x0c00, 0x0c7f, Text3DCharacterRangeType.Telugu),
            // new(0x0c80, 0x0cff, Text3DCharacterRangeType.Kannada),
            // new(0x0d00, 0x0d7f, Text3DCharacterRangeType.Malayalam),
            // new(0x0d80, 0x0dff, Text3DCharacterRangeType.Sinhala),
            // new(0x0e00, 0x0e7f, Text3DCharacterRangeType.Thai),
            // new(0x0e80, 0x0eff, Text3DCharacterRangeType.Lao),
            // new(0x0f00, 0x0fff, Text3DCharacterRangeType.Tibetan),
            // new(0x1000, 0x109f, Text3DCharacterRangeType.Myanmar),
            // new(0x10a0, 0x10ff, Text3DCharacterRangeType.Georgian),
            // new(0x1100, 0x11ff, Text3DCharacterRangeType.HangulJamo),
            // new(0x1200, 0x137f, Text3DCharacterRangeType.Ethiopic),
            // new(0x1380, 0x139f, Text3DCharacterRangeType.EthiopicSupplement),
            // new(0x13a0, 0x13ff, Text3DCharacterRangeType.Cherokee),
            // new(0x1400, 0x167f, Text3DCharacterRangeType.UnifiedCanadianAboriginalSyllabics),
            // new(0x1680, 0x169f, Text3DCharacterRangeType.Ogham),
            // new(0x16a0, 0x16ff, Text3DCharacterRangeType.Runic),
            // new(0x1700, 0x171f, Text3DCharacterRangeType.Tagalog),
            // new(0x1720, 0x173f, Text3DCharacterRangeType.Hanunoo),
            // new(0x1740, 0x175f, Text3DCharacterRangeType.Buhid),
            // new(0x1760, 0x177f, Text3DCharacterRangeType.Tagbanwa),
            // new(0x1780, 0x17ff, Text3DCharacterRangeType.Khmer),
            // new(0x1800, 0x18af, Text3DCharacterRangeType.Mongolian),
            // new(0x18b0, 0x18ff, Text3DCharacterRangeType.UnifiedCanadianAboriginalSyllabicsExtended),
            // new(0x1900, 0x194f, Text3DCharacterRangeType.Limbu),
            // new(0x1950, 0x197f, Text3DCharacterRangeType.TaiLe),
            // new(0x1980, 0x19Df, Text3DCharacterRangeType.NewTaiLue),
            // new(0x19e0, 0x19ff, Text3DCharacterRangeType.KhmerSymbols),
            // new(0x1a00, 0x1a1f, Text3DCharacterRangeType.Buginese),
            // new(0x1a20, 0x1aaf, Text3DCharacterRangeType.TaiTham),
            // new(0x1ab0, 0x1aff, Text3DCharacterRangeType.CombiningDiacriticalMarksExtended),
            // new(0x1b00, 0x1b7f, Text3DCharacterRangeType.Balinese),
            // new(0x1b80, 0x1bbf, Text3DCharacterRangeType.Sundanese),
            // new(0x1bc0, 0x1bff, Text3DCharacterRangeType.Batak),
            // new(0x1c00, 0x1c4f, Text3DCharacterRangeType.Lepcha),
            // new(0x1c50, 0x1c7f, Text3DCharacterRangeType.OlChiki),
            // new(0x1c80, 0x1c87, Text3DCharacterRangeType.CyrillicExtendedC),
            // new(0x1cc0, 0x1ccf, Text3DCharacterRangeType.SundaneseSupplement),
            // new(0x1cd0, 0x1cff, Text3DCharacterRangeType.VedicExtensions),
            // new(0x1d00, 0x1d7f, Text3DCharacterRangeType.PhoneticExtensions),
            // new(0x1d80, 0x1dbf, Text3DCharacterRangeType.PhoneticExtensionsSupplement),
            // new(0x1dc0, 0x1dff, Text3DCharacterRangeType.CombiningDiacriticalMarksSupplement),
            // new(0x1e00, 0x1eff, Text3DCharacterRangeType.LatinExtendedAdditional),
            // new(0x1f00, 0x1fff, Text3DCharacterRangeType.GreekExtended),
            // new(0x2000, 0x206f, Text3DCharacterRangeType.GeneralPunctuation),
            // new(0x2070, 0x209f, Text3DCharacterRangeType.SuperscriptsAndSubscripts),
            // new(0x20a0, 0x20cf, Text3DCharacterRangeType.CurrencySymbols),
            // new(0x20d0, 0x20ff, Text3DCharacterRangeType.CombiningDiacriticalMarksForSymbols),
            // new(0x2100, 0x214f, Text3DCharacterRangeType.LetterlikeSymbols),
            // new(0x2150, 0x218f, Text3DCharacterRangeType.NumberForms),
            // new(0x2190, 0x21ff, Text3DCharacterRangeType.Arrows),
            // new(0x2200, 0x22ff, Text3DCharacterRangeType.MathematicalOperators),
            // new(0x2300, 0x23ff, Text3DCharacterRangeType.MiscellaneousTechnical),
            // new(0x2400, 0x243f, Text3DCharacterRangeType.ControlPictures),
            // new(0x2440, 0x245f, Text3DCharacterRangeType.OpticalCharacterRecognition),
            // new(0x2460, 0x24ff, Text3DCharacterRangeType.EnclosedAlphanumerics),
            // new(0x2500, 0x257f, Text3DCharacterRangeType.BoxDrawing),
            // new(0x2580, 0x259f, Text3DCharacterRangeType.BlockElements),
            // new(0x25a0, 0x25ff, Text3DCharacterRangeType.GeometricShapes),
            // new(0x2600, 0x26ff, Text3DCharacterRangeType.MiscellaneousSymbols),
            // new(0x2700, 0x27bf, Text3DCharacterRangeType.Dingbats),
            // new(0x27c0, 0x27ef, Text3DCharacterRangeType.MiscellaneousMathematicalSymbolsA),
            // new(0x27f0, 0x27ff, Text3DCharacterRangeType.SupplementalArrowsA),
            // new(0x2800, 0x28ff, Text3DCharacterRangeType.BraillePatterns),
            // new(0x2900, 0x297f, Text3DCharacterRangeType.SupplementalArrowsB),
            // new(0x2980, 0x29ff, Text3DCharacterRangeType.MiscellaneousMathematicalSymbolsB),
            // new(0x2a00, 0x2aff, Text3DCharacterRangeType.SupplementalMathematicalOperators),
            // new(0x2b00, 0x2bff, Text3DCharacterRangeType.MiscellaneousSymbolsAndArrows),
            // new(0x2c00, 0x2c5f, Text3DCharacterRangeType.Glagolitic),
            // new(0x2c60, 0x2c7f, Text3DCharacterRangeType.LatinExtendedC),
            // new(0x2c80, 0x2cff, Text3DCharacterRangeType.Coptic),
            // new(0x2d00, 0x2d2f, Text3DCharacterRangeType.GeorgianSupplement),
            // new(0x2d30, 0x2d7f, Text3DCharacterRangeType.Tifinagh),
            // new(0x2d80, 0x2ddf, Text3DCharacterRangeType.EthiopicExtended),
            // new(0x2de0, 0x2dff, Text3DCharacterRangeType.CyrillicExtendedA),
            // new(0x2e00, 0x2e7f, Text3DCharacterRangeType.SupplementalPunctuation),
            // new(0x2e80, 0x2eff, Text3DCharacterRangeType.CJKRadicalsSupplement),
            // new(0x2f00, 0x2fdf, Text3DCharacterRangeType.KangxiRadicals),
            // new(0x2ff0, 0x2fff, Text3DCharacterRangeType.IdeographicDescriptionCharacters),
            // new(0x3000, 0x303f, Text3DCharacterRangeType.CJKSymbolsAndPunctuation),
            // new(0x3040, 0x309f, Text3DCharacterRangeType.Hiragana),
            // new(0x30a0, 0x30ff, Text3DCharacterRangeType.Katakana),
            // new(0x3100, 0x312f, Text3DCharacterRangeType.Bopomofo),
            // new(0x3130, 0x318f, Text3DCharacterRangeType.HangulCompatibilityJamo),
            // new(0x3190, 0x319f, Text3DCharacterRangeType.Kanbun),
            // new(0x31a0, 0x31bf, Text3DCharacterRangeType.BopomofoExtended),
            // new(0x31c0, 0x31ef, Text3DCharacterRangeType.CJKStrokes),
            // new(0x31f0, 0x31ff, Text3DCharacterRangeType.KatakanaPhoneticExtensions),
            // new(0x3200, 0x32ff, Text3DCharacterRangeType.EnclosedCJKLettersAndMonths),
            // new(0x3300, 0x33ff, Text3DCharacterRangeType.CJKCompatibility),
            // new(0x3400, 0x4dbf, Text3DCharacterRangeType.CJKUnifiedIdeographsExtensionA),
            // new(0x4dc0, 0x4dff, Text3DCharacterRangeType.YijingHexagramSymbols),
            // new(0x4e00, 0x9fff, Text3DCharacterRangeType.CJKUnifiedIdeographs),
            // new(0xa000, 0xa48f, Text3DCharacterRangeType.YiSyllables),
            // new(0xa490, 0xa4cf, Text3DCharacterRangeType.YiRadicals),
            // new(0xa4d0, 0xa4ff, Text3DCharacterRangeType.Lisu),
            // new(0xa500, 0xa63f, Text3DCharacterRangeType.Vai),
            // new(0xa640, 0xa69f, Text3DCharacterRangeType.CyrillicExtendedB),
            // new(0xa6a0, 0xa6ff, Text3DCharacterRangeType.Bamum),
            // new(0xa700, 0xa71f, Text3DCharacterRangeType.ModifierToneLetters),
            // new(0xa720, 0xa7ff, Text3DCharacterRangeType.LatinExtendedD),
            // new(0xa800, 0xa82f, Text3DCharacterRangeType.SylotiNagri),
            // new(0xa830, 0xa83f, Text3DCharacterRangeType.CommonIndicNumberForms),
            // new(0xa840, 0xa87f, Text3DCharacterRangeType.PhagsPa),
            // new(0xa880, 0xa8df, Text3DCharacterRangeType.Saurashtra),
            // new(0xa8e0, 0xa8ff, Text3DCharacterRangeType.DevanagariExtended),
            // new(0xa900, 0xa92f, Text3DCharacterRangeType.KayahLi),
            // new(0xa930, 0xa95f, Text3DCharacterRangeType.Rejang),
            // new(0xa960, 0xa97f, Text3DCharacterRangeType.HangulJamoExtendedA),
            // new(0xa980, 0xa9df, Text3DCharacterRangeType.Javanese),
            // new(0xa9e0, 0xa9ff, Text3DCharacterRangeType.MyanmarExtendedB),
            // new(0xaa00, 0xaa5f, Text3DCharacterRangeType.Cham),
            // new(0xaa60, 0xaa7f, Text3DCharacterRangeType.MyanmarExtendedA),
            // new(0xaa80, 0xaadf, Text3DCharacterRangeType.TaiViet),
            // new(0xaae0, 0xaaff, Text3DCharacterRangeType.MeeteiMayekExtensions),
            // new(0xab00, 0xab2f, Text3DCharacterRangeType.EthiopicExtendedA),
            // new(0xab30, 0xab6f, Text3DCharacterRangeType.LatinExtendedE),
            // new(0xab70, 0xabbf, Text3DCharacterRangeType.CherokeeSupplement),
            // new(0xabc0, 0xabff, Text3DCharacterRangeType.MeeteiMayek),
            // new(0xac00, 0xd7af, Text3DCharacterRangeType.HangulSyllables),
            // new(0xd7b0, 0xd7ff, Text3DCharacterRangeType.HangulJamoExtendedB),
            // new(0xd800, 0xdb7f, Text3DCharacterRangeType.HighSurrogates),
            // new(0xdb80, 0xdbff, Text3DCharacterRangeType.HighPrivateUseSurrogates),
            // new(0xdc00, 0xdfff, Text3DCharacterRangeType.LowSurrogates),
            // new(0xe000, 0xf8ff, Text3DCharacterRangeType.PrivateUseArea),
            // new(0xf900, 0xfaff, Text3DCharacterRangeType.CJKCompatibilityIdeographs),
            // new(0xfb00, 0xfb4f, Text3DCharacterRangeType.AlphabeticPresentationForms),
            // new(0xfb50, 0xfdff, Text3DCharacterRangeType.ArabicPresentationFormsA),
            // new(0xfe00, 0xfe0f, Text3DCharacterRangeType.VariationSelectors),
            // new(0xfe10, 0xfe1f, Text3DCharacterRangeType.VerticalForms),
            // new(0xfe20, 0xfe2f, Text3DCharacterRangeType.CombiningHalfMarks),
            // new(0xfe30, 0xfe4f, Text3DCharacterRangeType.CJKCompatibilityForms),
            // new(0xfe50, 0xfe6f, Text3DCharacterRangeType.SmallFormVariants),
            // new(0xfe70, 0xfeff, Text3DCharacterRangeType.ArabicPresentationFormsB),
            // new(0xff00, 0xffef, Text3DCharacterRangeType.HalfwidthAndFullwidthForms),
            // new(0xfff0, 0xffff, Text3DCharacterRangeType.Specials),
            // new(0x10000, 0x1007f, Text3DCharacterRangeType.LinearBSyllabary),
            // new(0x10080, 0x100ff, Text3DCharacterRangeType.LinearBIdeograms),
            // new(0x10100, 0x1013f, Text3DCharacterRangeType.AegeanNumbers),
            // new(0x10140, 0x1018f, Text3DCharacterRangeType.AncientGreekNumbers),
            // new(0x10190, 0x101cf, Text3DCharacterRangeType.AncientSymbols),
            // new(0x101d0, 0x101ff, Text3DCharacterRangeType.PhaistosDisc),
            // new(0x10280, 0x1029f, Text3DCharacterRangeType.Lycian),
            // new(0x102a0, 0x102df, Text3DCharacterRangeType.Carian),
            // new(0x102e0, 0x102ff, Text3DCharacterRangeType.CopticEpactNumbers),
            // new(0x10300, 0x1032f, Text3DCharacterRangeType.OldItalic),
            // new(0x10330, 0x1034f, Text3DCharacterRangeType.Gothic),
            // new(0x10350, 0x1037f, Text3DCharacterRangeType.OldPermic),
            // new(0x10380, 0x1039f, Text3DCharacterRangeType.Ugaritic),
            // new(0x103a0, 0x103df, Text3DCharacterRangeType.OldPersian),
            // new(0x10400, 0x1044f, Text3DCharacterRangeType.Deseret),
            // new(0x10450, 0x1047f, Text3DCharacterRangeType.Shavian),
            // new(0x10480, 0x104af, Text3DCharacterRangeType.Osmanya),
            // new(0x104b0, 0x104ff, Text3DCharacterRangeType.Osage),
            // new(0x10500, 0x1052f, Text3DCharacterRangeType.Elbasan),
            // new(0x10530, 0x1056f, Text3DCharacterRangeType.CaucasianAlbanian),
            // new(0x10600, 0x1077f, Text3DCharacterRangeType.LinearA),
            // new(0x10800, 0x1083f, Text3DCharacterRangeType.CypriotSyllabary),
            // new(0x10840, 0x1085f, Text3DCharacterRangeType.ImperialAramaic),
            // new(0x10860, 0x1087f, Text3DCharacterRangeType.Palmyrene),
            // new(0x10880, 0x108af, Text3DCharacterRangeType.Nabataean),
            // new(0x108e0, 0x108ff, Text3DCharacterRangeType.Hatran),
            // new(0x10900, 0x1091f, Text3DCharacterRangeType.Phoenician),
            // new(0x10920, 0x1093f, Text3DCharacterRangeType.Lydian),
            // new(0x10980, 0x1099f, Text3DCharacterRangeType.MeroiticHieroglyphs),
            // new(0x109a0, 0x109ff, Text3DCharacterRangeType.MeroiticCursive),
            // new(0x10a00, 0x10a5f, Text3DCharacterRangeType.Kharoshthi),
            // new(0x10a60, 0x10a7f, Text3DCharacterRangeType.OldSouthArabian),
            // new(0x10a80, 0x10a9f, Text3DCharacterRangeType.OldNorthArabian),
            // new(0x10ac0, 0x10aff, Text3DCharacterRangeType.Manichaean),
            // new(0x10b00, 0x10b3f, Text3DCharacterRangeType.Avestan),
            // new(0x10b40, 0x10b5f, Text3DCharacterRangeType.InscriptionalParthian),
            // new(0x10b60, 0x10b7f, Text3DCharacterRangeType.InscriptionalPahlavi),
            // new(0x10b80, 0x10baf, Text3DCharacterRangeType.PsalterPahlavi),
            // new(0x10c00, 0x10c4f, Text3DCharacterRangeType.OldTurkic),
            // new(0x10c80, 0x10cff, Text3DCharacterRangeType.OldHungarian),
            // new(0x10e60, 0x10e7f, Text3DCharacterRangeType.RumiNumeralSymbols),
            // new(0x11000, 0x1107f, Text3DCharacterRangeType.Brahmi),
            // new(0x11080, 0x110cf, Text3DCharacterRangeType.Kaithi),
            // new(0x110d0, 0x110ff, Text3DCharacterRangeType.SoraSompeng),
            // new(0x11100, 0x1114f, Text3DCharacterRangeType.Chakma),
            // new(0x11150, 0x1117f, Text3DCharacterRangeType.Mahajani),
            // new(0x11180, 0x111df, Text3DCharacterRangeType.Sharada),
            // new(0x111e0, 0x111ff, Text3DCharacterRangeType.SinhalaArchaicNumbers),
            // new(0x11200, 0x1124f, Text3DCharacterRangeType.Khojki),
            // new(0x11280, 0x112af, Text3DCharacterRangeType.Multani),
            // new(0x112b0, 0x112ff, Text3DCharacterRangeType.Khudawadi),
            // new(0x11300, 0x1137f, Text3DCharacterRangeType.Grantha),
            // new(0x11400, 0x1147f, Text3DCharacterRangeType.Newa),
            // new(0x11480, 0x114df, Text3DCharacterRangeType.Tirhuta),
            // new(0x11580, 0x115ff, Text3DCharacterRangeType.Siddham),
            // new(0x11600, 0x1165f, Text3DCharacterRangeType.Modi),
            // new(0x11660, 0x1167f, Text3DCharacterRangeType.MongolianSupplement),
            // new(0x11680, 0x116cf, Text3DCharacterRangeType.Takri),
            // new(0x11700, 0x1173f, Text3DCharacterRangeType.Ahom),
            // new(0x118a0, 0x118ff, Text3DCharacterRangeType.WarangCiti),
            // new(0x11ac0, 0x11aff, Text3DCharacterRangeType.PauCinHau),
            // new(0x11c00, 0x11c6f, Text3DCharacterRangeType.Bhaiksuki),
            // new(0x11c70, 0x11cbf, Text3DCharacterRangeType.Marchen),
            // new(0x12000, 0x123ff, Text3DCharacterRangeType.Cuneiform),
            // new(0x12400, 0x1247f, Text3DCharacterRangeType.CuneiformNumbersAndPunctuation),
            // new(0x12480, 0x1254f, Text3DCharacterRangeType.EarlyDynasticCuneiform),
            // new(0x13000, 0x1342f, Text3DCharacterRangeType.EgyptianHieroglyphs),
            // new(0x14400, 0x1467f, Text3DCharacterRangeType.AnatolianHieroglyphs),
            // new(0x16800, 0x16a3f, Text3DCharacterRangeType.BamumSupplement),
            // new(0x16a40, 0x16a6f, Text3DCharacterRangeType.Mro),
            // new(0x16ad0, 0x16aff, Text3DCharacterRangeType.BassaVah),
            // new(0x16b00, 0x16b8f, Text3DCharacterRangeType.PahawhHmong),
            // new(0x16f00, 0x16f9f, Text3DCharacterRangeType.Miao),
            // new(0x16fe0, 0x16fff, Text3DCharacterRangeType.IdeographicSymbolsAndPunctuation),
            // new(0x17000, 0x187ff, Text3DCharacterRangeType.Tangut),
            // new(0x18800, 0x18aff, Text3DCharacterRangeType.TangutComponents),
            // new(0x1b000, 0x1b0ff, Text3DCharacterRangeType.KanaSupplement),
            // new(0x1bc00, 0x1bc9f, Text3DCharacterRangeType.Duployan),
            // new(0x1bca0, 0x1bcaf, Text3DCharacterRangeType.ShorthandFormatControls),
            // new(0x1d000, 0x1d0ff, Text3DCharacterRangeType.ByzantineMusicalSymbols),
            // new(0x1d100, 0x1d1ff, Text3DCharacterRangeType.MusicalSymbols),
            // new(0x1d200, 0x1d24f, Text3DCharacterRangeType.AncientGreekMusicalNotation),
            // new(0x1d300, 0x1d35f, Text3DCharacterRangeType.TaiXuanJingSymbols),
            // new(0x1d360, 0x1d37f, Text3DCharacterRangeType.CountingRodNumerals),
            // new(0x1d400, 0x1d7ff, Text3DCharacterRangeType.MathematicalAlphanumericSymbols),
            // new(0x1d800, 0x1daaf, Text3DCharacterRangeType.SuttonSignWriting),
            // new(0x1e000, 0x1e02f, Text3DCharacterRangeType.GlagoliticSupplement),
            // new(0x1e800, 0x1e8df, Text3DCharacterRangeType.MendeKikakui),
            // new(0x1e900, 0x1e95f, Text3DCharacterRangeType.Adlam),
            // new(0x1ee00, 0x1eeff, Text3DCharacterRangeType.ArabicMathematicalAlphabeticSymbols),
            // new(0x1f000, 0x1f02f, Text3DCharacterRangeType.MahjongTiles),
            // new(0x1f030, 0x1f09f, Text3DCharacterRangeType.DominoTiles),
            // new(0x1f0a0, 0x1f0ff, Text3DCharacterRangeType.PlayingCards),
            // new(0x1f100, 0x1f1ff, Text3DCharacterRangeType.EnclosedAlphanumericSupplement),
            // new(0x1f200, 0x1f2ff, Text3DCharacterRangeType.EnclosedIdeographicSupplement),
            // new(0x1f300, 0x1f5ff, Text3DCharacterRangeType.MiscellaneousSymbolsAndPictographs),
            // new(0x1f600, 0x1f64f, Text3DCharacterRangeType.EmoticonsEmoji),
            // new(0x1f650, 0x1f67f, Text3DCharacterRangeType.OrnamentalDingbats),
            // new(0x1f680, 0x1f6ff, Text3DCharacterRangeType.TransportAndMapSymbols),
            // new(0x1f700, 0x1f77f, Text3DCharacterRangeType.AlchemicalSymbols),
            // new(0x1f780, 0x1f7ff, Text3DCharacterRangeType.GeometricShapesExtended),
            // new(0x1f800, 0x1f8ff, Text3DCharacterRangeType.SupplementalArrowsC),
            // new(0x1f900, 0x1f9ff, Text3DCharacterRangeType.SupplementalSymbolsAndPictographs),
            // new(0x20000, 0x2a6d6, Text3DCharacterRangeType.CJKUnifiedIdeographsExtensionB),
            // new(0x2a700, 0x2b734, Text3DCharacterRangeType.CJKUnifiedIdeographsExtensionC),
            // new(0x2b740, 0x2b81d, Text3DCharacterRangeType.CJKUnifiedIdeographsExtensionD),
            // new(0x2b820, 0x2cea1, Text3DCharacterRangeType.CJKUnifiedIdeographsExtensionE),
            // new(0x2f800, 0x2fa1f, Text3DCharacterRangeType.CJKCompatibilityIdeographsSupplement),
            // new(0xe0000, 0xe007f, Text3DCharacterRangeType.Tags),
            // new(0xe0100, 0xe01ef, Text3DCharacterRangeType.VariationSelectorsSupplement)
        };

        private static readonly int Hash = "Text3D".GetHashCode();

        // private static bool _fontShow = true;
        private static bool _meshShow = false;


        private ReorderableList _characterRangeList = null;
        private PreviewRenderUtility _previewUtility = null;
        private Vector2 _previewDir = Vector2.zero;
        private float _zoom = 0.0f;

        private bool _changeCheck;

        private SerializedProperty _glyphList;

        private SerializedProperty _characterRanges;

        // private SerializedProperty faceMaterial;
        // private SerializedProperty sideMaterial;
        // private SerializedProperty outlineMaterial;
        private SerializedProperty _meshCompression;
        private SerializedProperty _outlineJoin;
        private SerializedProperty _topLeftColor;
        private SerializedProperty _topRightColor;
        private SerializedProperty _bottomLeftColor;
        private SerializedProperty _bottomRightColor;
        private SerializedProperty _assetPath;
        private SerializedProperty _curveQuality;
        private SerializedProperty _extrude;
        private SerializedProperty _outlineWidth;
        private SerializedProperty _miterLimit;
        private SerializedProperty _bevelSegments;
        private SerializedProperty _unitsPerEm;
        private SerializedProperty _missingGlyph;
        private SerializedProperty _optimizeMesh;
        private SerializedProperty _useTangents;
        private SerializedProperty _useColors;
        private SerializedProperty _uvCorrection;


        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Character Ranges");
        }

        void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var element = _characterRangeList.serializedProperty.GetArrayElementAtIndex(index);
            var custom = element.FindPropertyRelative("custom");
            var begin = element.FindPropertyRelative("begin");
            var end = element.FindPropertyRelative("end");
            var type = element.FindPropertyRelative("type");
            var left = rect.x;
            var width = rect.width;

            // GUI.enabled = string.IsNullOrEmpty(custom.stringValue);
            EditorGUI.BeginChangeCheck();

            rect.width = width * 0.55f;
            EditorGUI.PropertyField(rect, type, new GUIContent(), true);

            if (EditorGUI.EndChangeCheck() && (int) Text3DCharacterRangeType.Custom != type.enumValueIndex)
            {
                begin.intValue = Ranges[type.enumValueIndex].begin;
                end.intValue = Ranges[type.enumValueIndex].end;
                _changeCheck = true;
            }

            EditorGUI.BeginChangeCheck();
            GUI.enabled = type.enumValueIndex != (int) Text3DCharacterRangeType.Custom;

            rect.x += rect.width + 5.0f;
            rect.width = width * 0.2f;
            EditorGUI.PropertyField(rect, begin, new GUIContent(), true);

            rect.x += rect.width + 5.0f;
            rect.width = width * 0.2f;
            EditorGUI.PropertyField(rect, end, new GUIContent(), true);

            if (EditorGUI.EndChangeCheck())
            {
                // type.enumValueIndex = (int) Text3DCharacterRangeType.Custom;
                _changeCheck = true;
            }

            GUI.enabled = type.enumValueIndex == (int) Text3DCharacterRangeType.Custom;
            EditorGUI.BeginChangeCheck();

            rect.x = left;
            rect.y += EditorGUIUtility.singleLineHeight + 1.0f;
            rect.width = width * 0.55f;
            EditorGUI.PropertyField(rect, custom, new GUIContent(), true);

            if (EditorGUI.EndChangeCheck())
                _changeCheck = true;

            GUI.enabled = type.enumValueIndex != (int) Text3DCharacterRangeType.Custom;
            EditorGUI.BeginChangeCheck();

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x += rect.width + 5.0f;
            rect.width = width * 0.2f;
            begin.intValue = EditorGUI.IntField(rect, begin.intValue);

            rect.x += rect.width + 5.0f;
            rect.width = width * 0.2f;
            end.intValue = EditorGUI.IntField(rect, end.intValue);

            if (EditorGUI.EndChangeCheck())
            {
                type.enumValueIndex = (int) Text3DCharacterRangeType.Custom;
                _changeCheck = true;
            }

            GUI.enabled = true;
        }

        public void OnEnable()
        {
            Text3DFont asset;

            if (serializedObject.isEditingMultipleObjects)
            {
                foreach (Object t in targets)
                {
                    asset = t as Text3DFont;
                    asset.CreateSnapshot();
                }
            }
            else
            {
                asset = target as Text3DFont;
                asset.CreateSnapshot();
            }

            _glyphList = serializedObject.FindProperty("glyphList");
            _characterRanges = serializedObject.FindProperty("characterRangeList");
            // faceMaterial = serializedObject.FindProperty("faceMaterial");
            // sideMaterial = serializedObject.FindProperty("sideMaterial");
            // outlineMaterial = serializedObject.FindProperty("outlineMaterial");
            _meshCompression = serializedObject.FindProperty("meshCompression");
            _outlineJoin = serializedObject.FindProperty("outlineJoin");
            _topLeftColor = serializedObject.FindProperty("topLeftColor");
            _topRightColor = serializedObject.FindProperty("topRightColor");
            _bottomLeftColor = serializedObject.FindProperty("bottomLeftColor");
            _bottomRightColor = serializedObject.FindProperty("bottomRightColor");
            _assetPath = serializedObject.FindProperty("assetPath");
            _curveQuality = serializedObject.FindProperty("curveQuality");
            _extrude = serializedObject.FindProperty("extrude");
            _outlineWidth = serializedObject.FindProperty("outlineWidth");
            _miterLimit = serializedObject.FindProperty("miterLimit");
            _bevelSegments = serializedObject.FindProperty("bevelSegments");
            _unitsPerEm = serializedObject.FindProperty("unitsPerEm");
            _missingGlyph = serializedObject.FindProperty("missingGlyph");
            _optimizeMesh = serializedObject.FindProperty("optimizeMesh");
            _useTangents = serializedObject.FindProperty("useTangents");
            _useColors = serializedObject.FindProperty("useColors");
            _uvCorrection = serializedObject.FindProperty("uvCorrection");
            _changeCheck = false;

            if (null == _characterRangeList)
            {
                _characterRangeList = new ReorderableList(serializedObject, _characterRanges, false, true, true, true) {elementHeight = EditorGUIUtility.singleLineHeight * 2.0f + 5.0f, drawHeaderCallback = DrawHeader, drawElementCallback = DrawElement};
            }

            if (null == _previewUtility)
                _previewUtility = new PreviewRenderUtility();

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        private void OnDisable()
        {
            if (_changeCheck)
            {
                var asset = "";

                if (serializedObject.isEditingMultipleObjects)
                    asset += targets.Length;
                else
                    asset = AssetDatabase.GetAssetPath(target);

                if (EditorUtility.DisplayDialog("Unapplied import settings", "Unapplied import settings for '" + asset + "'", "Apply", "Revert"))
                    ApplyChanges();
                else
                    RevertChanges();
            }

            _previewUtility.Cleanup();

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
        }

        private void OnDestroy()
        {
            _characterRangeList = null;
            _previewUtility = null;
        }

        private static void OnSceneGUI(SceneView view)
        {
            var type = Event.current.type;

            if (EventType.DragUpdated == type)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                Event.current.Use();
            }
            else if (EventType.DragPerform == type)
            {
                Text3D.Scripts.Text3D actor;
                GameObject go;
                Vector3 pos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).GetPoint(5.0f);

                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is Text3DFont)
                    {
                        go = new GameObject(obj.name);

                        go.transform.position = pos;

                        actor = go.AddComponent<Text3D.Scripts.Text3D>();
                        actor.SetFont(obj as Text3DFont, false);

                        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                        Selection.activeGameObject = go;
                    }
                }

                Event.current.Use();
            }
        }

        private static void HierarchyWindowItemOnGUI(int id, Rect selectionRect)
        {
            var type = Event.current.type;

            if (EventType.DragUpdated != type && EventType.DragPerform != type && EventType.DragExited != type)
                return;

            Text3D.Scripts.Text3D actor;
            GameObject go;

            DragAndDrop.visualMode = DragAndDropVisualMode.Link;

            if (selectionRect.Contains(Event.current.mousePosition) && EventType.DragPerform == type)
            {
                var parent = EditorUtility.InstanceIDToObject(id) as GameObject;

                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is not Text3DFont font) continue;
                    go = new GameObject(font.name);

                    if (null == go) continue;
                    go.transform.SetParent(parent.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    actor = go.AddComponent<Text3D.Scripts.Text3D>();
                    actor.SetFont(font, false);

                    Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                    Selection.activeGameObject = go;
                }

                Event.current.Use();
            }
            else if (!selectionRect.Contains(Event.current.mousePosition) && EventType.DragExited == type)
            {
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is not Text3DFont font) continue;
                    go = new GameObject(font.name);

                    if (null == go) continue;
                    actor = go.AddComponent<Text3D.Scripts.Text3D>();
                    actor.SetFont(font, false);

                    Selection.activeGameObject = go;
                    Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
                    Undo.PerformRedo();
                }

                Event.current.Use();
            }
        }

        public override string GetInfoString()
        {
            var asset = target as Text3DFont;
            return null == asset ? "" : asset.InfoString;
        }

        private void DoOpenFile()
        {
            var label = new GUIContent("Font Name");
            var position = EditorGUILayout.GetControlRect(true, 16.0f);
            var id = GUIUtility.GetControlID(Hash, FocusType.Keyboard, position);
            var evt = Event.current;
            var type = evt.type;

            position = EditorGUI.PrefixLabel(position, id, label);

            switch (type)
            {
                case EventType.MouseDown when 0 == Event.current.button && position.Contains(Event.current.mousePosition):
                {
                    EditorGUIUtility.editingTextField = false;

                    string[] filters = {"All files", "ttf,otf", "TrueType fonts", "ttf", "OpenType fonts", "otf"};
                    var directory = string.IsNullOrEmpty(_assetPath.stringValue) ? Directory.GetCurrentDirectory().Replace('\\', '/') + "/" : System.IO.Path.GetDirectoryName(_assetPath.stringValue);
                    var path = EditorUtility.OpenFilePanelWithFilters("Open File", directory, filters);

                    if (!string.IsNullOrEmpty(path))
                    {
                        _assetPath.stringValue = path;
                        _changeCheck = true;
                        serializedObject.ApplyModifiedProperties();
                    }

                    evt.Use();
                    GUIUtility.ExitGUI();
                    break;
                }
                case EventType.Repaint:
                    EditorStyles.objectField.Draw(position, new GUIContent(_assetPath.stringValue.Substring(_assetPath.stringValue.LastIndexOf("/") + 1)), id, false);
                    break;
                case EventType.MouseUp:
                    break;
                case EventType.MouseMove:
                    break;
                case EventType.MouseDrag:
                    break;
                case EventType.KeyDown:
                    break;
                case EventType.KeyUp:
                    break;
                case EventType.ScrollWheel:
                    break;
                case EventType.Layout:
                    break;
                case EventType.DragUpdated:
                    break;
                case EventType.DragPerform:
                    break;
                case EventType.DragExited:
                    break;
                case EventType.Ignore:
                    break;
                case EventType.Used:
                    break;
                case EventType.ValidateCommand:
                    break;
                case EventType.ExecuteCommand:
                    break;
                case EventType.ContextClick:
                    break;
                case EventType.MouseEnterWindow:
                    break;
                case EventType.MouseLeaveWindow:
                    break;
                case EventType.TouchDown:
                    break;
                case EventType.TouchUp:
                    break;
                case EventType.TouchMove:
                    break;
                case EventType.TouchEnter:
                    break;
                case EventType.TouchLeave:
                    break;
                case EventType.TouchStationary:
                    break;
                // default:
                //     throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // _fontShow = EditorGUILayout.Foldout(_fontShow, "Font");
            // if (_fontShow)
            // {
            DoOpenFile();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_missingGlyph);
            EditorGUILayout.Space();

            _characterRangeList.DoLayoutList();
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
                _changeCheck = true;
            // }

            _meshShow = EditorGUILayout.Foldout(_meshShow, "Mesh");
            if (_meshShow)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(_curveQuality);
                EditorGUILayout.PropertyField(_extrude);
                EditorGUILayout.PropertyField(_outlineWidth);
                EditorGUILayout.PropertyField(_bevelSegments);
                EditorGUILayout.PropertyField(_miterLimit);
                EditorGUILayout.PropertyField(_outlineJoin);
                EditorGUILayout.PropertyField(_meshCompression);
                EditorGUILayout.PropertyField(_optimizeMesh);
                EditorGUILayout.PropertyField(_useTangents);
                EditorGUILayout.PropertyField(_useColors);
                EditorGUILayout.PropertyField(_uvCorrection);
                // EditorGUILayout.PropertyField(faceMaterial);
                // EditorGUILayout.PropertyField(sideMaterial);
                // EditorGUILayout.PropertyField(outlineMaterial);
                EditorGUILayout.PropertyField(_topLeftColor);
                EditorGUILayout.PropertyField(_topRightColor);
                EditorGUILayout.PropertyField(_bottomLeftColor);
                EditorGUILayout.PropertyField(_bottomRightColor);

                if (EditorGUI.EndChangeCheck())
                    _changeCheck = true;

                EditorGUILayout.PropertyField(_glyphList);
                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Replace Meshes", GUILayout.MinWidth(55.0f)))
                {
                    ReplaceMeshes();
                }

                GUILayout.EndHorizontal();
            }

            GUI.enabled = _changeCheck && !EditorApplication.isPlayingOrWillChangePlaymode;
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Revert", GUILayout.MinWidth(55.0f)))
                RevertChanges();

            if (GUILayout.Button("Apply", GUILayout.MinWidth(50.0f)))
                ApplyChanges();

            GUILayout.EndHorizontal();
        }

        private void ReplaceMeshes()
        {
            Save(SaveGlyphMesh);
        }

        private void Save(Action<Text3DFont> onSave)
        {
            Text3DFont asset;
            var progress = 0.0f;

            if (serializedObject.isEditingMultipleObjects)
            {
                AssetDatabase.StartAssetEditing();

                foreach (var t in targets)
                {
                    asset = t as Text3DFont;
                    onSave?.Invoke(asset);
                    progress += (1.0f / targets.Length);
                    EditorUtility.DisplayProgressBar("Apply", "Apply Change for '" + AssetDatabase.GetAssetPath(t) + "'", progress);
                }

                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }
            else
            {
                asset = target as Text3DFont;
                onSave?.Invoke(asset);
            }

            _changeCheck = false;

            GUI.FocusControl(null);
            EditorGUI.FocusTextInControl(null);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Canvas.ForceUpdateCanvases();
            SceneView.RepaintAll();
        }

        private static void SaveGlyphMesh(Text3DFont asset)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

            foreach (var a in assets)
            {
                if (a is Mesh)
                {
                    DestroyImmediate(a, true);
                }
            }

            foreach (var glyph in asset.GlyphList)
            {
                glyph.mesh = (Mesh) UnityEngine.Object.Instantiate(glyph.mesh);
                glyph.mesh.name = asset.name.Trim() + "_" + glyph.id + "_" + (char) glyph.id;
                AssetDatabase.AddObjectToAsset(glyph.mesh, asset);
            }
        }

        public override bool HasPreviewGUI()
        {
            var asset = target as Text3DFont;

            return null != asset;
        }


        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                if (Event.current.type == EventType.Repaint)
                    EditorGUI.DropShadowLabel(new Rect(rect.x, rect.y, rect.width, 40), "TextTools preview requires\nrender texture support");

                return;
            }

            // if (null == _previewUtility)
            //     _previewUtility = new PreviewRenderUtility();

            var id = GUIUtility.GetControlID(Hash, FocusType.Passive);
            var ev = Event.current;

            switch (ev.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (rect.Contains(ev.mousePosition) && rect.width > 50)
                    {
                        GUIUtility.hotControl = id;
                        ev.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id && 0 == ev.button)
                    {
                        _previewDir -= ev.delta * (ev.shift ? 3 : 1) / Mathf.Min(rect.width, rect.height) * 140.0f;
                        _previewDir.y = Mathf.Clamp(_previewDir.y, -90, 90);
                        ev.Use();
                        GUI.changed = true;
                    }
                    else if (GUIUtility.hotControl == id && 1 == ev.button)
                    {
                        _zoom -= ev.delta.y * (ev.shift ? 3 : 1) / Mathf.Min(rect.width, rect.height) * 140.0f;
                        _zoom += ev.delta.x * (ev.shift ? 3 : 1) / Mathf.Min(rect.width, rect.height) * 140.0f;
                        ev.Use();
                        GUI.changed = true;
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                        GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseMove:
                    break;
                case EventType.KeyDown:
                    break;
                case EventType.KeyUp:
                    break;
                case EventType.ScrollWheel:
                    break;
                case EventType.Repaint:
                    break;
                case EventType.Layout:
                    break;
                case EventType.DragUpdated:
                    break;
                case EventType.DragPerform:
                    break;
                case EventType.DragExited:
                    break;
                case EventType.Ignore:
                    break;
                case EventType.Used:
                    break;
                case EventType.ValidateCommand:
                    break;
                case EventType.ExecuteCommand:
                    break;
                case EventType.ContextClick:
                    break;
                case EventType.MouseEnterWindow:
                    break;
                case EventType.MouseLeaveWindow:
                    break;
                case EventType.TouchDown:
                    break;
                case EventType.TouchUp:
                    break;
                case EventType.TouchMove:
                    break;
                case EventType.TouchEnter:
                    break;
                case EventType.TouchLeave:
                    break;
                case EventType.TouchStationary:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // if (Event.current.type != EventType.Repaint)
            //     return;

//             var asset = target as Text3DFont;
//             var rot = Quaternion.Euler(_previewDir.y, 0, 0) * Quaternion.Euler(0, _previewDir.x, 0);
//             var scale = 72.0f / (float) _unitsPerEm.intValue;
//
// #if UNITY_2017_1_OR_NEWER
//
//             var camera = _previewUtility.camera;
//             var lights = _previewUtility.lights;
// #else
// 			Camera        camera = _previewUtility.m_Camera;
// 			Light[]       lights = _previewUtility.m_Light;
//
// #endif


            // if (null == asset.PreviewGlyphs) // || null == asset.Materials)
            //     return;
            //
            // _previewUtility.BeginPreview(rect, background);
            //
            // camera.fieldOfView = 30.0f;
            // camera.nearClipPlane = 0.1f;
            // camera.farClipPlane = 10000.0f;
            // camera.clearFlags = CameraClearFlags.Nothing;
            // camera.transform.position = Vector3.back;
            // camera.transform.rotation = Quaternion.identity;
            //
            // lights[0].intensity = 1.4f;
            // lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
            // lights[1].intensity = 1.4f;
            //
            // for (var i = 0; i < asset.PreviewGlyphs.Length; ++i)
            // {
            //     if (null == asset.PreviewGlyphs[i])
            //         continue;
            //
            //     var pos = rot * new Vector3(asset.PreviewPositions[i].x * scale, asset.PreviewPositions[i].y * scale, _extrude.floatValue * 0.5f * scale);
            //     pos.z -= asset.PreviewPositions[i].z * (rect.height / rect.width) * scale + _zoom;
            //
            //     _previewUtility.DrawMesh(asset.PreviewGlyphs[i].mesh, Matrix4x4.TRS(pos, rot, new Vector3(scale, scale, scale)), null, 0);
            // }
            //
            // camera.Render();
            // var tex = _previewUtility.EndPreview();
            // EditorGUI.DrawPreviewTexture(rect, tex, null, ScaleMode.StretchToFill);
        }

        public override Texture2D RenderStaticPreview(string path, UnityEngine.Object[] subassets, int width, int height)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
                return null;

            _previewUtility ??= new PreviewRenderUtility();

            var asset = target as Text3DFont;
            var rot = Quaternion.Euler(_previewDir.y, 0, 0) * Quaternion.Euler(0, _previewDir.x, 0);
            var scale = 72.0f / (float) _unitsPerEm.intValue;

#if UNITY_2017_1_OR_NEWER

            var camera = _previewUtility.camera;
            var lights = _previewUtility.lights;
#else
			Camera        camera = previewUtility.m_Camera;
			Light[]       lights = previewUtility.m_Light;

#endif

            if (asset is not null && null == asset.PreviewGlyphs) // || null == asset.Materials)
                return null;

            _previewUtility.BeginStaticPreview(new Rect(0.0f, 0.0f, width, height));

            camera.fieldOfView = 30.0f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10000.0f;
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.transform.position = Vector3.back;
            camera.transform.rotation = Quaternion.identity;

            lights[0].intensity = 1.4f;
            lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
            lights[1].intensity = 1.4f;

            for (var i = 0; i < asset.PreviewGlyphs.Length; ++i)
            {
                if (null == asset.PreviewGlyphs[i])
                    continue;

                var pos = rot * new Vector3(asset.PreviewPositions[i].x * scale, asset.PreviewPositions[i].y * scale, _extrude.floatValue * 0.5f * scale);
                pos.z -= asset.PreviewPositions[i].z * scale;

                // for (int j = 0; j < asset.Materials.Length; ++j)
                //     previewUtility.DrawMesh(asset.PreviewGlyphs[i].mesh, Matrix4x4.TRS(pos, rot, new Vector3(scale, scale, scale)), asset.Materials[j], j);
            }

            camera.Render();

            return _previewUtility.EndStaticPreview();
        }

        private void RevertChanges()
        {
            Text3DFont asset = null;

            if (serializedObject.isEditingMultipleObjects)
            {
                foreach (Object t in targets)
                {
                    asset = t as Text3DFont;
                    asset.Revert();
                }
            }
            else
            {
                asset = target as Text3DFont;
                asset.Revert();
            }

            _changeCheck = false;

            GUI.FocusControl(null);
            EditorGUI.FocusTextInControl(null);
        }

        private void ApplyChanges()
        {
            Save(asset => { asset.Apply(TextToolsLoader.LoadFont, TextToolsLoader.LoadKerningPairs, TextToolsCreator.CreateGlyph); });
        }

        [MenuItem("Assets/Create/TextTools Font", false, 100)]
        private static void CreateTextToolsFont()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if ("" == path)
                path = "Assets";
            else if ("" != System.IO.Path.GetExtension(path))
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

            var asset = CreateInstance<Text3DFont>();
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path + "/TextToolsFont.asset"));

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
        }

        private void ExportFBX()
        {
            var path = Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(target)) ?? string.Empty, $"{target.name}.fbx");
            var newGO = new GameObject();
            // newGO.AddComponent<MeshFilter>().sharedMesh = 
            if (!string.IsNullOrEmpty(ModelExporter.ExportObject(path, null)))
            {
            }
            DestroyImmediate(newGO);
        }
    }
}