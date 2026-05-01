import { useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useSelector } from "react-redux";
import type { RootState } from "../app/store";

export function ProfileRedirect() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, isAuthenticated, loading } = useSelector(
    (state: RootState) => state.auth,
  );

  useEffect(() => {
    if (loading) return;

    // check if redirect is needed
    if (
      isAuthenticated &&
      user &&
      !user.isProfileComplete &&
      !location.pathname.includes("/profile")
    ) {
      console.log("Redirecting to profile completion");
      navigate("/profile", { replace: true });
    }
  }, [user, isAuthenticated, loading, location.pathname, navigate]);

  return null; // without render
}
