import React from "react";
import Backdrop from "@mui/material/Backdrop";
import CircularProgress from "@mui/material/CircularProgress";
import { useTheme } from "@mui/material/styles";

interface LoadingOverlayProps {
  loading: boolean;
}

const LoadingOverlay: React.FC<LoadingOverlayProps> = ({ loading }) => {
  const theme = useTheme();

  return (
    <>
      {loading && (
        <Backdrop
          sx={{
            color: theme.palette.mode === "dark" ? "#fff" : "#000",
            zIndex: (theme) => theme.zIndex.drawer + 10,
            position: "fixed",
            top: 0,
            left: 0,
            width: "100%",
            height: "100%",
            backgroundColor:
              theme.palette.mode === "dark"
                ? "rgba(0, 0, 0, 0.5)"
                : "rgba(255, 255, 255, 0.5)",
            backdropFilter: "blur(4px)", // додає розмиття
          }}
          open={true}
        >
          <CircularProgress color="inherit" />
        </Backdrop>
      )}
    </>
  );
};

export default LoadingOverlay;
