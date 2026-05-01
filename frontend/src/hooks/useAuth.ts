import { useSelector, useDispatch } from "react-redux";
import type { RootState, AppDispatch } from "../app/store";
import { fetchMe, logout } from "../features/auth/authSlice";

export function useAuth() {
  const dispatch = useDispatch<AppDispatch>();
  const { user, isAuthenticated, loading, error, isProfileComplete } =
    useSelector((state: RootState) => state.auth);

  const profileComplete = user?.isProfileComplete || isProfileComplete || false;

  return {
    user,
    isAuthenticated,
    loading,
    error,
    isProfileComplete: profileComplete,
    fetchMe: () => dispatch(fetchMe()),
    logout: () => dispatch(logout()),
  };
}
