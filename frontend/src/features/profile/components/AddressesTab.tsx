import { Box, Button, Card, Grid, Typography } from "@mui/material";
import type { ProfileAddressDto } from "../../../api/profileService";

interface Props {
  addresses: ProfileAddressDto[];
  onEdit: (address: ProfileAddressDto) => void;
  onDelete: (id: number) => void;
  onAdd: () => void;
}

export default function AddressesTab({
  addresses,
  onEdit,
  onDelete,
  onAdd,
}: Props) {
  return (
    <Box p={2}>
      <Typography variant="h6" mb={2}>
        Addresses
      </Typography>
      <Button variant="outlined" size="small" onClick={onAdd}>
        + Add Address
      </Button>
      <Grid container spacing={2} mt={1}>
        {addresses
          .filter((c: ProfileAddressDto) => c.isActive)
          .sort((a, b) => a.addressTypeId - b.addressTypeId)
          .map((address: ProfileAddressDto, index: number) => (
            <Grid key={address.id || index} size={{ xs: 12, md: 6 }}>
              <Card variant="outlined" sx={{ p: 2 }}>
                <Typography variant="subtitle2">
                  {address.addessTypeName}
                </Typography>
                <Typography>
                  {address.addressLine}, {address.city}, {address.zipCode},{" "}
                  {address.countryName}
                </Typography>
                <Box mt={1}>
                  <Button size="small" onClick={() => onEdit(address)}>
                    Edit
                  </Button>
                  <Button
                    size="small"
                    color="error"
                    onClick={() => onDelete(address.id!)}
                  >
                    Delete
                  </Button>
                </Box>
              </Card>
            </Grid>
          ))}
      </Grid>
    </Box>
  );
}
