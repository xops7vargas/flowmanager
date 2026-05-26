import { Box, Typography } from '@mui/material';

interface LogoProps {
  variant?: 'full' | 'icon';
  sx?: any;
}

const CatIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
    <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8 0-.46.04-.92.1-1.36.56.64 1.39 1.14 2.4 1.36 0-.64.08-1.27.22-1.88.32-.02.66-.08 1-.18-.12-.48-.18-.98-.18-1.5 0-1.93 1.13-3.55 2.75-4.2.3.34.7.6 1.15.74.02-.44.1-.86.25-1.26-.1-.2-.16-.42-.16-.66 0-.18.04-.36.1-.52.46.62 1.14 1.1 1.96 1.38.3-.68.48-1.42.48-2.2 0-1.1-.41-2.1-1.09-2.86.08-.2.14-.42.14-.64 0-.23-.05-.45-.14-.66.68-.58 1.55-.92 2.49-.92.18 0 .36.01.53.04-.04-.28-.06-.56-.06-.85 0-1.86 1.28-3.41 3.02-3.87L17 3c-.68-.16-1.4-.24-2.13-.24-3.13 0-5.82 1.82-7.13 4.47C6.44 4.77 5.23 7.34 5.23 10c0 .72.12 1.41.33 2.06C4.25 11.4 3 12.56 3 14c0 1.66 1.34 3 3 3 .42 0 .82-.09 1.18-.24-.07-.24-.11-.49-.11-.76 0-.91.39-1.73 1-2.35-.22-.06-.45-.11-.69-.14.48-.66 1.21-1.15 2.05-1.32-.42-.18-.87-.28-1.34-.28-.14 0-.27.01-.41.03C5.73 8.08 7.52 6.5 9.88 6.5c.9 0 1.74.26 2.46.7.88-.32 1.88-.5 2.91-.5 1.23 0 2.4.38 3.41 1.02-.12-.02-.24-.04-.37-.04-1.38 0-2.5 1.12-2.5 2.5 0 .19.02.38.06.56-.75.34-1.28.99-1.28 1.8 0 .61.28 1.15.71 1.48-.1.01-.21.02-.32.02-.66 0-1.25-.24-1.71-.63.28.67.44 1.4.44 2.17 0 1.84-1.01 3.38-2.47 3.96-.3.12-.62.21-.95.26-.17.66-.56 1.23-1.09 1.62-.48.35-1.05.52-1.64.47-.38-.04-.73-.15-1.04-.31.42.13.87.2 1.33.2z"/>
  </svg>
);

export const Logo = ({ variant = 'full', sx }: LogoProps) => {
  if (variant === 'icon') {
    return (
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          width: 40,
          height: 40,
          borderRadius: 2,
          background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)',
          color: 'white',
          ...sx,
        }}
      >
        <CatIcon />
      </Box>
    );
  }

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1,
        ...sx,
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          width: 40,
          height: 40,
          borderRadius: 2,
          background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)',
          color: 'white',
        }}
      >
        <CatIcon />
      </Box>
      <Typography
        variant="h6"
        sx={{
          fontWeight: 700,
          background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
          letterSpacing: '-0.02em',
        }}
      >
        Sonyi-Flow
      </Typography>
    </Box>
  );
};
