import React from 'react';
import { IconButton, Tooltip } from '@mui/material';
import { useTranslation } from 'react-i18next';
import LightModeIcon from '@mui/icons-material/LightMode';
import DarkModeIcon from '@mui/icons-material/DarkMode';
import { useTheme } from '../hooks/useApp';

export const ThemeToggle = () => {
  const { t } = useTranslation();
  const { mode, toggleTheme } = useTheme();

  return (
    <Tooltip title={mode === 'light' ? t('common.darkMode') : t('common.lightMode')}>
      <IconButton
        onClick={toggleTheme}
        sx={{
          transition: 'transform 0.3s ease',
          '&:hover': {
            transform: 'rotate(30deg)',
          },
        }}
      >
        {mode === 'light' ? <DarkModeIcon /> : <LightModeIcon />}
      </IconButton>
    </Tooltip>
  );
};
