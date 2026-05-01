import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import LoadingOverlay from "./ui/LoadingOverlay";

export function UnauthenticatedRoute() {
  const { isAuthenticated, loading, isProfileComplete } = useAuth();

  if (loading) {
    return <LoadingOverlay loading={true} />;
  }

  if (isAuthenticated) {
    return <Navigate to={isProfileComplete ? "/" : "/profile"} replace />;
  }

  return <Outlet />;
}
