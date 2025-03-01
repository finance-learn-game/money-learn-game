using System;

namespace uPalette.Generated
{
public enum ColorTheme
    {
        light,
        light_medium_contrast,
        light_high_contrast,
        dark,
        dark_medium_contrast,
        dark_high_contrast,
    }

    public static class ColorThemeExtensions
    {
        public static string ToThemeId(this ColorTheme theme)
        {
            switch (theme)
            {
                case ColorTheme.light:
                    return "bf8630a4-ddb4-4ded-a96a-1c2caf28b7b4";
                case ColorTheme.light_medium_contrast:
                    return "b36af7f1-b85b-4f2b-9e32-299735f59204";
                case ColorTheme.light_high_contrast:
                    return "9d921526-9aaa-4f17-b3c0-3433f10a11f6";
                case ColorTheme.dark:
                    return "3a6105dd-30ce-4ace-9cb4-36693eda77c4";
                case ColorTheme.dark_medium_contrast:
                    return "bc6e862d-5c8d-40df-b031-4287f5dc17df";
                case ColorTheme.dark_high_contrast:
                    return "7b8f57fc-b784-4569-8b21-e302ab0f0c0e";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum ColorEntry
    {
        primary,
        surfaceTint,
        onPrimary,
        primaryContainer,
        onPrimaryContainer,
        secondary,
        onSecondary,
        secondaryContainer,
        onSecondaryContainer,
        tertiary,
        onTertiary,
        tertiaryContainer,
        onTertiaryContainer,
        error,
        onError,
        errorContainer,
        onErrorContainer,
        background,
        onBackground,
        surface,
        onSurface,
        surfaceVariant,
        onSurfaceVariant,
        outline,
        outlineVariant,
        shadow,
        scrim,
        inverseSurface,
        inverseOnSurface,
        inversePrimary,
        primaryFixed,
        onPrimaryFixed,
        primaryFixedDim,
        onPrimaryFixedVariant,
        secondaryFixed,
        onSecondaryFixed,
        secondaryFixedDim,
        onSecondaryFixedVariant,
        tertiaryFixed,
        onTertiaryFixed,
        tertiaryFixedDim,
        onTertiaryFixedVariant,
        surfaceDim,
        surfaceBright,
        surfaceContainerLowest,
        surfaceContainerLow,
        surfaceContainer,
        surfaceContainerHigh,
        surfaceContainerHighest,
        divider,
    }

    public static class ColorEntryExtensions
    {
        public static string ToEntryId(this ColorEntry entry)
        {
            switch (entry)
            {
                case ColorEntry.primary:
                    return "d1f1d4ab-ce28-4aed-ac85-9d3dc4dbfcb8";
                case ColorEntry.surfaceTint:
                    return "07d201cf-5385-4ed1-b8df-33ef5e3a3892";
                case ColorEntry.onPrimary:
                    return "551cc79e-8a89-4ecd-9dd9-2af9a74bc70f";
                case ColorEntry.primaryContainer:
                    return "abaf3e32-ede9-4d68-bb25-81651eda1979";
                case ColorEntry.onPrimaryContainer:
                    return "061d46b1-05fa-4988-a860-1504225c4cbb";
                case ColorEntry.secondary:
                    return "a86a6678-9513-4c60-b6cc-3f63c4a4ac2a";
                case ColorEntry.onSecondary:
                    return "faaa3edf-ddfc-4061-9883-c7e662830376";
                case ColorEntry.secondaryContainer:
                    return "9fea1664-c050-47bd-ab77-a9e6a9382370";
                case ColorEntry.onSecondaryContainer:
                    return "3fd3c2c7-37c3-4f1b-93b0-08c9c5df475e";
                case ColorEntry.tertiary:
                    return "bca668d1-5431-443d-aabb-3e75c42e4748";
                case ColorEntry.onTertiary:
                    return "d7b3b966-5875-4bea-8f57-e130667f78c9";
                case ColorEntry.tertiaryContainer:
                    return "741888ac-ffa5-44c4-94b4-976ae9c4f390";
                case ColorEntry.onTertiaryContainer:
                    return "e9695f03-2bd0-4009-a251-931ed45ed434";
                case ColorEntry.error:
                    return "9c78ba17-b4c7-44bb-b175-67069f58e0a2";
                case ColorEntry.onError:
                    return "dccf497a-61c9-4134-b5ea-6ec68639a084";
                case ColorEntry.errorContainer:
                    return "c5dcc159-8dc8-4a58-adcd-f79880d807e4";
                case ColorEntry.onErrorContainer:
                    return "48574586-1a0d-45bd-a354-52097e39cf0d";
                case ColorEntry.background:
                    return "b44ab376-3c26-458a-9fa9-b89e0c6f4ebe";
                case ColorEntry.onBackground:
                    return "c7300456-2c96-467d-864d-070c3267c3c3";
                case ColorEntry.surface:
                    return "a1b48c57-aae7-4786-a597-eefbdf4f326f";
                case ColorEntry.onSurface:
                    return "8dc34384-cf61-4d47-87cb-d3c37d826acc";
                case ColorEntry.surfaceVariant:
                    return "42639afd-186d-4e6a-b92e-2c8f39b35b6b";
                case ColorEntry.onSurfaceVariant:
                    return "4e67beed-869f-4dcf-bd90-7d31821e63ed";
                case ColorEntry.outline:
                    return "839d6eba-b986-46f2-916b-f85e84b0cd42";
                case ColorEntry.outlineVariant:
                    return "c1c3178f-b84f-4fa7-bcfc-dab6dd1df0e5";
                case ColorEntry.shadow:
                    return "1b45c552-f4fa-4ea8-908f-719fdc1e86ab";
                case ColorEntry.scrim:
                    return "b100567e-8768-4ccc-90ba-8c7429fd7cc5";
                case ColorEntry.inverseSurface:
                    return "63ece8f8-3904-485b-9db9-b6478e2c41c7";
                case ColorEntry.inverseOnSurface:
                    return "7a212256-04d6-4740-b0ee-923bcd723556";
                case ColorEntry.inversePrimary:
                    return "bc4d5fa4-70c9-4930-ab7f-2762f0c93a7b";
                case ColorEntry.primaryFixed:
                    return "ae252d34-5b9b-4421-9c34-51909370131d";
                case ColorEntry.onPrimaryFixed:
                    return "a08cbfa1-92b0-4167-b787-d100fa7b430c";
                case ColorEntry.primaryFixedDim:
                    return "a3636ff8-d5cf-4b97-b26a-09f9e393619c";
                case ColorEntry.onPrimaryFixedVariant:
                    return "d1d00cde-f39b-48a3-8934-766cba94b9ff";
                case ColorEntry.secondaryFixed:
                    return "58abcc9a-10ef-4685-9b67-b7a68733c392";
                case ColorEntry.onSecondaryFixed:
                    return "bc639dea-493a-4bac-b2a7-952853715aea";
                case ColorEntry.secondaryFixedDim:
                    return "23352913-4dca-495f-adc4-a952aacf0154";
                case ColorEntry.onSecondaryFixedVariant:
                    return "f1377c88-2b4a-41a2-8465-7f32210f8814";
                case ColorEntry.tertiaryFixed:
                    return "4e36f864-9305-4174-b605-0f60b88c2d55";
                case ColorEntry.onTertiaryFixed:
                    return "4e5d63c2-8b4c-43fa-b7ad-5e5e9cea13a9";
                case ColorEntry.tertiaryFixedDim:
                    return "45983d85-7eaa-4db5-ade8-1671f3b2cdb8";
                case ColorEntry.onTertiaryFixedVariant:
                    return "0a6fca91-fd0c-4981-a2ea-044fd5ced380";
                case ColorEntry.surfaceDim:
                    return "ec507ebb-bd85-4cc2-8d25-36ed48ace9d4";
                case ColorEntry.surfaceBright:
                    return "93c39538-4e11-4d98-b983-9a79451c728d";
                case ColorEntry.surfaceContainerLowest:
                    return "6e5e42a1-8b0c-4308-aec7-533a8fca3fe9";
                case ColorEntry.surfaceContainerLow:
                    return "fd657434-21b5-4a4d-a580-b35bdb64a410";
                case ColorEntry.surfaceContainer:
                    return "17446685-4cca-4e5d-a681-26b5389afaa9";
                case ColorEntry.surfaceContainerHigh:
                    return "a7574792-2532-4d0b-976e-cc045ea4260e";
                case ColorEntry.surfaceContainerHighest:
                    return "c3410dc1-b188-4b79-9277-b4c196fb7dc9";
                case ColorEntry.divider:
                    return "838d9eff-aeeb-4bd9-a438-3eff6c9ce2ec";
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum GradientTheme
    {
        Default,
    }

    public static class GradientThemeExtensions
    {
        public static string ToThemeId(this GradientTheme theme)
        {
            switch (theme)
            {
                case GradientTheme.Default:
                    return "c0cbb0b7-9aff-4cac-8f29-a1dfa7eeecba";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum GradientEntry
    {
    }

    public static class GradientEntryExtensions
    {
        public static string ToEntryId(this GradientEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTheme
    {
        Default,
    }

    public static class CharacterStyleThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTheme.Default:
                    return "71a8f291-6385-4541-9a2c-74de4165ee6d";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleEntry
    {
    }

    public static class CharacterStyleEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTMPTheme
    {
        Default,
    }

    public static class CharacterStyleTMPThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTMPTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTMPTheme.Default:
                    return "86d3f870-6652-478c-9fb3-7aa694ec7a5f";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleTMPEntry
    {
    }

    public static class CharacterStyleTMPEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleTMPEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }
}
