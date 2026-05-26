import { useSelector, useDispatch } from 'react-redux';
import type { RootState, AppDispatch } from '../store';
import { toggleTheme, setTheme } from '../features/theme/themeSlice';

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector = <T>(selector: (state: RootState) => T) => useSelector(selector);
export const useTheme = () => {
  const dispatch = useAppDispatch();
  const mode = useAppSelector((state) => state.theme.mode);
  
  return {
    mode,
    toggleTheme: () => dispatch(toggleTheme()),
    setTheme: (theme: 'light' | 'dark') => dispatch(setTheme(theme)),
  };
};
