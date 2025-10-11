// can be AxiosError, Fetch error, or rejectWithValue payload
export function getErrorMessage(error: any): string {
  if (!error) return "Unknown error";

  // if received from thunkAPI.rejectWithValue({ title, detail })
  if (typeof error === "object") {
    if (error.detail) return error.detail;
    if (error.title) return error.title;
    if (error.message) return error.message;
  }

  // Axios standard error
  if (error.response?.data) {
    const data = error.response.data;
    if (data.detail) return data.detail;
    if (data.title) return data.title;
    if (data.message) return data.message;
  }

  // if simple text
  if (typeof error === "string") return error;

  return "Something went wrong";
}
